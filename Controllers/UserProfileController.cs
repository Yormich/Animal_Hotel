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
        private readonly IUserTypeService _userTypeService;

        public UserProfileController(ClaimHelper claimHelper, IUserLoginInfoService userLoginService, IMemoryCache cache,
            IIFileProvider fileProvider, IUserTypeService userTypeService)
        {
            _claimHelper = claimHelper;
            _userLoginService = userLoginService;
            _cache = cache; 
            _fileProvider = fileProvider;
            _userTypeService = userTypeService;
        }

        [HttpGet]
        [Authorize]
        public IActionResult UserProfile()
        {
            bool isClient = _claimHelper.HasClaimWithValue("Client");

            return RedirectToAction($"{(isClient ? "Client" : "Employee")}PersonalData");
        }

        [HttpGet]
        [Authorize(Roles = "Client")]
        [ActionMapper("ClientPersonalData", "UserProfile", "Personal Data")]
        public async Task<IActionResult> ClientPersonalData()
        {
            long userId = Convert.ToInt64(_claimHelper.GetClaimValue(ClaimTypes.PrimarySid));
            var actionsTask = UtilFuncs.CreateUserActionsList(_claimHelper.GetClaimValue(ClaimTypes.Role), _cache);
            var client = await _userLoginService.GetClientDataById(userId);

            if (client == null)
            {
                return NotFound();
            }

            client.Actions = await actionsTask;
            client.ActiveAction = "ClientPersonalData";

            return View(client);
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> UpdateClient(ClientDataViewModel model)
        {
            if (!UtilFuncs.IsUserOldEnough(model.BirthDate, 16))
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
        public async Task<IActionResult> UpdateEmployee(EmployeeDataViewModel model)
        {
            if (!UtilFuncs.IsUserOldEnough(model.BirthDate, 18))
            {
                ModelState.AddModelError("BirthDate", "You should be older 18 to work in Animal Hotel");
            }

            await HandleUsersSimilarData(model);

            if (!ModelState.IsValid)
            {
                return View("EmployeePersonalData", model);
            }

            Response.StatusCode = await _userLoginService.UpdateEmployeePersonalData(model) ? 200 : 501;
            return RedirectToAction("EmployeePersonalData", "UserProfile", new { userId = model.UserId });
        }

        private async Task HandleUsersSimilarData(UserViewModel model)
        {
            var actionsBuilder = UtilFuncs.CreateUserActionsList(_claimHelper.GetClaimValue(ClaimTypes.Role), _cache);
            await UpdatePasswordIfChanged(model);
            await UpdateProfileFileIfPassed(model, HttpContext.Request.Form.Files,
                (await _userLoginService.GetUserPhotoPathById(model.UserId))!);
            await CheckUserUniqueData(model);
            var userTypeT = _userTypeService.GetUserTypeByUserId(model.UserId);
            model.Actions = await actionsBuilder;
            model.UserType = await userTypeT;
        }

        [HttpGet]
        [Authorize(Roles = "AnimalWatcher,Receptionist,HotelManager")]
        [ActionMapper("EmployeePersonalData", "UserProfile", "Personal Data")]
        public async Task<IActionResult> EmployeePersonalData()
        {
            long userId = Convert.ToInt64(_claimHelper.GetClaimValue(ClaimTypes.PrimarySid));
            var actionsTask = UtilFuncs.CreateUserActionsList(_claimHelper.GetClaimValue(ClaimTypes.Role), _cache);
            var employee = await _userLoginService.GetEmployeeDataById(userId);


            if (employee == null)
            {
                return NotFound();
            }

            employee.Actions = await actionsTask;
            employee.ActiveAction = "EmployeePersonalData";

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

        private async Task CheckUserUniqueData(UserViewModel model)
        {
            //because in profile we can update only phone number, we passing empty string in email to verificate only phone number
            var userDataCheckResult = await _userLoginService.IsUserWithPhoneAndEmailExists(string.Empty, model.PhoneNumber);

            if (userDataCheckResult.isExists)
            {
                ModelState.AddModelError("PhoneNumber", "User with this phone number already exists");
            }
        }
    }
}
