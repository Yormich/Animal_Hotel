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
        private readonly IRoomService _roomService;
        private readonly IEnclosureService _enclosureService;

        public HotelManagerController(ClaimHelper claimHelper, IUserTypeService userTypeService, IMemoryCache memoryCache,
            IEmployeeService employeeService, IUserLoginInfoService userSerice, IIFileProvider fileProvider,
            IRequestService requestService, IRoomService roomService, IEnclosureService enclosureService)
        {
            _claimHelper = claimHelper;
            _userTypeService = userTypeService;
            _memoryCache = memoryCache;
            _employeeService = employeeService;
            _userService = userSerice;
            _fileProvider = fileProvider;
            _requestService = requestService;
            _roomService = roomService;
            _enclosureService = enclosureService;
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
                return View("RegisterEmployee", await RecoverModelFromRegister(newEmployee));
            }

            IFormFileCollection files = HttpContext.Request.Form.Files;
            await this.HandleEmployeePhotoFiles(newEmployee, files);

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
        [ActionMapper("HotelRooms", "HotelManager", "Rooms")]
        public async Task<IActionResult> HotelRooms(int? pageIndex)
        {
            int index = pageIndex ?? 1, pageSize = 10;
            HotelManagerViewModel manager = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache))
            {
                ActiveAction = "HotelRooms",
                Rooms = new PaginatedList<Room>(await _roomService.GetManagerRoomsByPageIndex(index, pageSize),
                    await _roomService.GetRoomsCountAsync(true), index, pageSize),
            };
            return View("RoomsManagement", manager);
        }

        [HttpGet]
        [Authorize(Roles = "HotelManager")]
        public async Task<IActionResult> ManagerRoomInformation(short roomId, long? enclosureId, long? employeeId)
        {
            HotelManagerViewModel manager = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache))
            {
                ActiveRoom = await _roomService.GetManagerRoomInfo(roomId),
            };

            if (enclosureId != null)
            {
                manager.IsInteractedWithModal = true;
                manager.ActiveEnclosure = await _enclosureService.GetEnclosureById(enclosureId);
            }

            if (employeeId != null)
            {
                manager.IsInteractedWithModal = true;
                manager.ActiveEmployee = await _employeeService.GetEmployeeById(employeeId);
            }

            return View("RoomFullInfo", manager);
        }

        [HttpGet]
        [Authorize(Roles = "HotelManager")]
        public async Task<IActionResult> RoomCreationPage(int pageIndex)
        {
            HotelManagerViewModel manager = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache))
            {
                ActiveRoom = new(),
                RoomTypes = await _roomService.GetRoomTypes(),
            };

            TempData[$"{manager.UserId}_aroom"] = pageIndex;

            return View("AddRoom", manager);
        }

        [HttpPost]
        [Authorize(Roles = "HotelManager")]
        public async Task<IActionResult> CreateRoom(HotelManagerViewModel model)
        {
            Room activeRoom = model.ActiveRoom!;
            IFormFile? file = HandleRoomPhotoFiles(HttpContext.Request.Form.Files);
            
            activeRoom.PhotoPath = file?.FileName ?? string.Empty;
            if (!ModelState.IsValid)
            {
                model = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache))
                {
                    ActiveRoom = activeRoom,
                    RoomTypes = await _roomService.GetRoomTypes(),
                };
                return View("AddRoom", model);
            }

            short insertedId = await _roomService.CreateRoom(activeRoom);
            await _fileProvider.UploadFileToServer(file!, $"room_{insertedId}_{file!.FileName}");

            int pageIndex = (int)TempData[$"{model.UserId}_aroom"]!;

            return RedirectToAction("HotelRooms", new {pageIndex});
        }

        [HttpGet]
        [Authorize(Roles = "HotelManager")]
        public async Task<IActionResult> EditRoomPage(short roomId)
        {
            HotelManagerViewModel manager = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache))
            {
                ActiveRoom = await _roomService.GetRoomBaseInfoById(roomId),
                RoomTypes = await _roomService.GetRoomTypes(),
            };

            return View("EditRoom", manager);
        }

        [HttpPost]
        [Authorize(Roles = "HotelManager")]
        public async Task<IActionResult> UpdateRoom(HotelManagerViewModel model)
        {
            Room updatedRoom = model.ActiveRoom!;
            var files = HttpContext.Request.Form.Files;

            if (files.Any())
            {
                var file = files[0];
                if (_fileProvider.IsFileExtensionSupported(file.FileName) && ModelState.IsValid)
                {
                    await _fileProvider.RemoveFileFromServer($"room_{updatedRoom.Id}_{file!.FileName}");
                    updatedRoom.PhotoPath = file.FileName;
                    await _fileProvider.UploadFileToServer(file, $"room_{updatedRoom.Id}_{file!.FileName}");
                }
                else
                {
                    ModelState.AddModelError("NewEmployee.PhotoPath", "This file extension is not supported");
                }
            }

            if (!ModelState.IsValid)
            {
                model = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache))
                {
                    ActiveRoom = updatedRoom,
                    RoomTypes = await _roomService.GetRoomTypes(),
                };

                return View("EditRoom", model);
            }

            //TODO: transaction
            await _roomService.UpdateRoom(updatedRoom);
            await _roomService.RemoveNotPreferrableEmployees(updatedRoom.Id);

            return RedirectToAction("ManagerRoomInformation", new {roomId = model.ActiveRoom!.Id});
        }

        [HttpGet]
        [Authorize(Roles = "HotelManager")]
        public async Task<IActionResult> AddResponsibleEmployee(short roomId)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Authorize(Roles = "HotelManager")]
        public async Task<IActionResult> AddAnimalEnclosureToRoom(short roomId)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Authorize(Roles = "HotelManager")]
        public async Task<IActionResult> DeleteRoom(short roomId)
        {
            if (await _roomService.IsRoomHasAnyActiveContractsOrBookings(roomId))
            {
                TempData["roomDeleteErrorMessage"] = "You can't delete room until there is any booking or active contract." +
                    " Try to to make room unable to be booked and wait till booking and contracts expire.";

                return RedirectToAction("ManagerRoomInformation", new { roomId });
            }
            await _roomService.RemoveRoom(roomId);

            return RedirectToAction("HotelRooms");
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

        private IFormFile? HandleRoomPhotoFiles(IFormFileCollection files)
        {
            IFormFile? file = null;
            if (files.Any())
            {
                var fileToCheck = files[0];
                if (!_fileProvider.IsFileExtensionSupported(fileToCheck.FileName))
                {
                    ModelState.AddModelError("ActiveRoom.PhotoPath", "This file extension is not supported");
                }
                else
                {
                    file = files[0];
                }
            }

            return file;
        }

        private async Task HandleEmployeePhotoFiles(EmployeeRegisterModel newEmployee, IFormFileCollection files)
        {
            if (files.Any())
            {
                var file = files[0];
                if (_fileProvider.IsFileExtensionSupported(file.FileName))
                {
                    newEmployee.PhotoPath = file.FileName;
                    await _fileProvider.UploadFileToServer(file, $"{newEmployee.Login}_{file.FileName}");
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
