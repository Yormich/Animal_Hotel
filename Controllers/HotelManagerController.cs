using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Models.ViewModels.RegisterViewModels;
using Animal_Hotel.Models.ViewModels.RoleViewModels;
using Animal_Hotel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;

namespace Animal_Hotel.Controllers
{
    public class HotelManagerController : Controller
    {
        private readonly ClaimHelper _claimHelper;
        private readonly IUserTypeService _userTypeService;
        private readonly IMemoryCache _memoryCache;
        private readonly IEmployeeService _employeeService;
        private readonly IUserLoginInfoService _userService;
        private readonly IIFileProvider _fileProvider;
        private readonly IRequestService _requestService;

        public HotelManagerController(ClaimHelper claimHelper, IUserTypeService userTypeService, IMemoryCache memoryCache,
            IEmployeeService employeeService, IUserLoginInfoService userSerice, IIFileProvider fileProvider,
            IRequestService requestService)
        {
            _claimHelper = claimHelper;
            _userTypeService = userTypeService;
            _memoryCache = memoryCache;
            _employeeService = employeeService;
            _userService = userSerice;
            _fileProvider = fileProvider;
            _requestService = requestService;
        }

        [HttpGet]
        [Authorize(Roles = "HotelManager")]
        [ActionMapper("GetEmployees", "HotelManager", "Employees")]
        public async Task<IActionResult> GetEmployees(int? pageIndex, long? employeeId)
        {
            HotelManagerViewModel manager = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache)) 
            {
                ActiveAction = "GetEmployees",
            };
            int pageSize = 8, index = pageIndex ?? 1;

            manager.Employees = new PaginatedList<EmployeeDataViewModel>(
                await _employeeService.GetEmployees(index, pageSize),
                await _employeeService.GetEmployeesCount(), index, pageSize);

            if (employeeId != null)
            {
                manager.EmployeeTypes = await _employeeService.GetEmployeePositions();
                manager.ActiveEmployee = await _employeeService.GetEmployeeById(employeeId);
                manager.IsInteractedWithModal = true;
            }

            return View("Employees", manager);
        }

        [HttpGet]
        [Authorize(Roles = "HotelManager")]
        public async Task<IActionResult> RegisterEmployeeView(int pageIndex)
        {
            HotelManagerViewModel manager = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache))
            {
                NewEmployee = new(),
                EmployeeTypes = await _employeeService.GetEmployeePositions(),
            };
            TempData[$"emp_a{manager.UserId}"] = pageIndex;
            return View("RegisterEmployee", manager);
        }

        [HttpPost]
        [Authorize(Roles = "HotelManager")]
        public async Task<IActionResult> RegisterEmployee(HotelManagerViewModel model)
        {
            EmployeeRegisterModel newEmployee = model.NewEmployee!;

            await CheckEmployeeLoginAndPhone(newEmployee);

            if (!UtilFuncs.IsUserOldEnough(newEmployee.BirthDate, 18))
            {
                ModelState.AddModelError("NewEmployee.BirthDate", "Employee must be older than 18");
            }

            if (!ModelState.IsValid)
            {
                foreach(var en in ModelState)
                {
                    Console.WriteLine(en.Key);
                    foreach(var e in en.Value.Errors)
                    {
                        Console.WriteLine($"\t{e.ErrorMessage}");
                    }
                }
                return View("RegisterEmployee", await RecoverModelFromRegister(newEmployee));
            }

            IFormFileCollection files = HttpContext.Request.Form.Files;
            if (files.Any())
            {
                var file = HttpContext.Request.Form.Files[0];
                if (_fileProvider.IsFileExtensionSupported(file.FileName))
                {
                    newEmployee.PhotoPath = file.FileName;
                    await _fileProvider.UploadFileToServer(file, $"{model.Login}_{file.FileName}");
                }
                else
                {
                    ModelState.AddModelError("NewEmployee.PhotoPath", "This file extension is not supported");
                }
            }
            else
            {
                ModelState.AddModelError("NewEmployee.PhotoPath", "Employee photo required for registration");
            }

            if (!ModelState.IsValid)
            {
                return View("RegisterEmployee", await RecoverModelFromRegister(newEmployee));
            }

            await _employeeService.RegisterEmployee(newEmployee);

            int pageIndex = (int)TempData[$"emp_a{model.UserId}"]!;
            return RedirectToAction("GetEmployees", new { pageIndex });
        }

        [HttpPost]
        [Authorize(Roles = "HotelManager")]
        public async Task<IActionResult> UpdateEmployee(HotelManagerViewModel manager, int pageIndex)
        {
            EmployeeDataViewModel employee = manager.ActiveEmployee!;

            if (!ModelState.IsValid)
            {
                int pageSize = 10;
                manager = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache))
                {
                    Employees = new(await _employeeService.GetEmployees(pageIndex, pageSize), 
                    await _employeeService.GetEmployeesCount(), pageIndex, pageSize),
                    EmployeeTypes = await _employeeService.GetEmployeePositions(),
                    ActiveEmployee = await _employeeService.GetEmployeeById(employee.SubUserId),
                    IsInteractedWithModal = true,
                };

                return View("Employees", manager);
            }

            if (!(await _employeeService.UpdateEmployeeByManager(employee)))
            {
                Response.StatusCode = 501;
            }

            return RedirectToAction("GetEmployees", new { pageIndex});
        }

        [HttpGet]
        [Authorize(Roles = "HotelManager")]
        public async Task<IActionResult> FireEmployee(long employeeId)
        {
            await _employeeService.DeleteEmployee(employeeId);

            return RedirectToAction("GetEmployees");
        }


        [HttpGet]
        [Authorize(Roles = "HotelManager")]
        public async Task<IActionResult> HotelRooms(int? pageIndex)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Authorize(Roles = "HotelManager")]
        public async Task<IActionResult> RoomInformation(short roomId)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Authorize(Roles = "HotelManager")]
        [ActionMapper("GetEmployeesRequests", "HotelManager", "Employees Requests")]
        public async Task<IActionResult> GetEmployeesRequests(int? pageIndex, long? requestId)
        {
            int index = pageIndex ?? 1, pageSize = 15;

            var requests = await _requestService.GetRequestsForManager(index, pageSize);
            int requestsCount = await _requestService.GetAllRequestsCount();
            HotelManagerViewModel manager = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache))
            {
                Requests = new(requests, requestsCount, index, pageSize),
                ActiveAction = "GetEmployeesRequests",
            };

            if(requestId != null)
            {
                manager.ActiveRequest = await _requestService.GetRequestById(requestId);
                TempData[$"mr_{manager.UserId}"] = pageIndex;
                manager.RequestStatuses = await _requestService.GetRequestStatusesForUpdate();
                manager.IsInteractedWithModal = true;
            }

            return View("EmployeesRequests", manager);
        }


        [HttpPost]
        [Authorize(Roles = "HotelManager")]
        public async Task<IActionResult> ChangeRequestStatus(HotelManagerViewModel model)
        {
            int pageIndex = (int)TempData[$"mr_{model.UserId}"]!;

            await _requestService.UpdateRequestStatus(model.ActiveRequest!.Id, model.ActiveRequest!.StatusId);

            return RedirectToAction("GetEmployeesRequests", new { pageIndex, requestId = model.ActiveRequest!.Id });
        }

        private async Task<HotelManagerViewModel> RecoverModelFromRegister(EmployeeRegisterModel newEmployee)
        {
            HotelManagerViewModel model = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache))
            {
                NewEmployee = newEmployee,
                EmployeeTypes = await _employeeService.GetEmployeePositions(),
            };

            return model;
        }

        private async Task CheckEmployeeLoginAndPhone(EmployeeRegisterModel employee)
        {
            var checkResult = await _userService.IsUserWithPhoneAndEmailExists(employee.Login ?? string.Empty, 
                employee.PhoneNumber ?? string.Empty);

            if (checkResult.isExists)
            {
                ModelState.AddModelError("NewEmployee.Login", checkResult.errorMessage);
                ModelState.AddModelError("NewEmployee.PhoneNumber", checkResult.errorMessage);
            }
        }
    }
}
