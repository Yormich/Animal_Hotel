using Animal_Hotel.Models.ViewModels.RoleViewModels;
using Animal_Hotel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Animal_Hotel.Controllers
{
    public class HotelManagerController : Controller
    {
        private readonly ClaimHelper _claimHelper;
        private readonly IUserTypeService _userTypeService;
        private readonly IMemoryCache _memoryCache;
        private readonly IEmployeeService _employeeService;

        public HotelManagerController(ClaimHelper claimHelper, IUserTypeService userTypeService, IMemoryCache memoryCache,
            IEmployeeService employeeService)
        {
            _claimHelper = claimHelper;
            _userTypeService = userTypeService;
            _memoryCache = memoryCache;
            _employeeService = employeeService;
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
            int pageSize = 10, index = pageIndex ?? 1;

            manager.Employees = new PaginatedList<EmployeeDataViewModel>(
                await _employeeService.GetEmployees(index, pageSize),
                await _employeeService.GetEmployeesCount(), index, pageSize);

            if (employeeId != null)
            {
                manager.ActiveEmployee = await _employeeService.GetEmployeeById(employeeId);
            }

            return View("Employees", manager);
        }

        [HttpGet]
        [Authorize(Roles = "HotelManager")]
        public async Task<IActionResult> RegisterEmployeeView()
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Authorize(Roles = "HotelManager")]
        public async Task<IActionResult> RegisterEmployee()
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Authorize(Roles = "HotelManager")]
        public async Task<IActionResult> FireEmployee(long employeeId)
        {
            throw new NotImplementedException();
        }
    }
}
