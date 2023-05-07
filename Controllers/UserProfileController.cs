using Animal_Hotel.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Server.IIS.Core;
using System.Reflection.Metadata.Ecma335;
using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Models.ViewModels.RoleViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.FileProviders;
using System.Text;

namespace Animal_Hotel.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly ClaimHelper _claimHelper;
        private readonly IUserLoginInfoService _userLoginService;
        private readonly IIFileProvider _fileProvider;

        public UserProfileController(ClaimHelper claimHelper, IUserLoginInfoService userLoginService, IMemoryCache cache,
            IIFileProvider fileProvider)
        {
            _claimHelper = claimHelper;
            _userLoginService = userLoginService;
            _cache = cache; 
            _fileProvider = fileProvider;
        }

        [HttpGet]
        [Authorize]
        public IActionResult UserProfile(long userId)
        {
            bool isClient = _claimHelper.HasClaimWithValue("Client");
            var routeData = new { userId };

            return RedirectToAction($"{(isClient ? "Client" : "Employee")}PersonalData", routeData);
        }

        [HttpGet]
        [Authorize(Roles = "Client")]
        [ActionMapper("ClientPersonalData", "UserProfile", "Personal Data")]
        public async Task<IActionResult> ClientPersonalData(long userId)
        {
            var actionsTask = CreateUserActionsList(_claimHelper.GetClaimValue(ClaimTypes.Role));
            var client = await _userLoginService.GetClientDataById(userId);

            if (client == null)
            {
                return NotFound();
            }

            client.Actions = await actionsTask;
            return View(client);
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> UpdateClient(ClientViewModel model)
        {
            if ((DateTime.Now.Subtract(model.BirthDate).Days / 365) < 16)
            {
                ModelState.AddModelError("BirthDate", "You should be older 16 to use client permissions");
            }

            await HandleUsersSimilarData(model);

            if (!ModelState.IsValid)
            {
                return View("ClientPersonalData", model);
            }

            Response.StatusCode = await _userLoginService.UpdateClient(model) ? 200 : 501;

            return RedirectToAction("ClientPersonalData","UserProfile", new {userId=model.UserId });
        }

        [HttpPost]
        [Authorize(Roles = "AnimalWatcher,Receptionist,HotelManager")]
        public async Task<IActionResult> UpdateEmployee(EmployeeViewModel model)
        {
            if ((DateTime.Now.Subtract(model.BirthDate).Days / 365) < 18)
            {
                ModelState.AddModelError("BirthDate", "You should be older 18 to work in Animal Hotel");
            }

            await HandleUsersSimilarData(model);

            if (!ModelState.IsValid)
            {
                foreach(var prop in ModelState)
                {
                    Console.WriteLine(prop.Key);
                    foreach(var err in prop.Value.Errors)
                    {
                        Console.WriteLine($"\t {err.ErrorMessage}");
                    }
                }
                return View("EmployeePersonalData", model);
            }

            Response.StatusCode = await _userLoginService.UpdateEmployeePersonalData(model) ? 200 : 501;
            return RedirectToAction("EmployeePersonalData", "UserProfile", new { userId = model.UserId });
        }

        private async Task HandleUsersSimilarData(UserViewModel model)
        {
            var actionsBuilder = this.CreateUserActionsList(_claimHelper.GetClaimValue(ClaimTypes.Role));
            await UpdatePasswordIfChanged(model);
            await UpdateProfileFileIfPassed(model, HttpContext.Request.Form.Files,
                (await _userLoginService.GetUserPhotoPathById(model.UserId))!);

            var userTypeT = _userLoginService.GetUserTypeByUserId(model.UserId);
            model.Actions = await actionsBuilder;
            model.UserType = await userTypeT;
        }

        [HttpGet]
        [Authorize(Roles = "AnimalWatcher,Receptionist,HotelManager")]
        [ActionMapper("EmployeePersonalData", "UserProfile", "Personal Data")]
        public async Task<IActionResult> EmployeePersonalData(long userId)
        {
            var actionsTask = CreateUserActionsList(_claimHelper.GetClaimValue(ClaimTypes.Role));
            var employee = await _userLoginService.GetEmployeeDataById(userId);


            if (employee == null)
            {
                return NotFound();
            }

            employee.Actions = await actionsTask;
            return View(employee);
        }

        private async Task UpdateProfileFileIfPassed(UserViewModel model, IFormFileCollection files, string photoPath)
        {
            //Restore photo path 
            model.PhotoPath = photoPath;

            //if any file passed
            if (files.Count != 0)
            {
                var file = files[0];
                //check file
                if (_fileProvider.IsFileExtensionSupported(file.FileName))
                {
                    await _fileProvider.RemoveFileFromServer($"{model.Login}_{model.PhotoPath!}");
                    model.PhotoPath = file.FileName;
                    await _fileProvider.UploadFileToServer(file, $"{model.Login}_{file.FileName}");
                    return;
                }

                //file hasn't passed condition
                ModelState.AddModelError("PhotoPath", "This file extension is not supported");
            }    
        }

        private Task UpdatePasswordIfChanged(UserViewModel model)
        {
            if (!string.IsNullOrEmpty(model.RepeatedPassword) && !string.IsNullOrEmpty(model.NewPassword))
            {
                string enteredPassword = UtilFuncs.Sha256_Hash(model.RepeatedPassword!);
                if (string.Compare(model.UserPassword, enteredPassword, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    ModelState.AddModelError("RepeatedPassword", "Passwords should match");
                    return Task.CompletedTask;
                }

                if (ModelState["NewPassword"]!.Errors.Count == 0)
                {
                    return _userLoginService.UpdatePasswordById(model.UserId, model.NewPassword!);
                }
            }
            return Task.CompletedTask;
        }

        private Task<Dictionary<string, (string Controller, string Display)>> CreateUserActionsList(string role)
        {
            return Task.Run(() =>
            {
                string key = $"{role}Actions";
                _cache.TryGetValue(key, out Dictionary<string, (string, string)>? cachedActions);

                if (cachedActions == null)
                {
                    var controller = typeof(Controller);

                    //get controllers that inherit from Controller class
                    var controllerDescendants = Assembly.GetExecutingAssembly().GetTypes()
                        .Where(t => controller.IsAssignableFrom(t));

                    //for each descendant
                    Dictionary<string, (string, string)> actions = new();
                    foreach (var descendant in controllerDescendants)
                    {
                        var methods = descendant
                            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                            .Where(method =>
                            {
                                var authorize = method.GetCustomAttribute<AuthorizeAttribute>();
                                bool isActionMapped = method.GetCustomAttribute<ActionMapperAttribute>() != null;

                                return authorize != null && isActionMapped
                                    && (authorize.Roles?.Contains(role, StringComparison.CurrentCulture) ?? false);
                            });
                        foreach (var method in methods)
                        {
                            var toAction = method.GetCustomAttribute<ActionMapperAttribute>()!;
                            actions.Add(toAction.ActionName, (toAction.ControllerName, toAction.DisplayName));
                        }
                    }

                    _cache.Set(key, actions, new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(30)));
                    return actions;
                }
                return cachedActions;

            });
        }
    }
}
