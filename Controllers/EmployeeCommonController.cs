using Animal_Hotel.Models.ViewModels.RoleViewModels;
using Animal_Hotel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Animal_Hotel.Models.DatabaseModels;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Animal_Hotel.Controllers
{
    public class EmployeeCommonController : Controller
    {
        private readonly IUserTypeService _userTypeService;
        private readonly IRequestService _requestService;
        private readonly ClaimHelper _claimHelper;
        private readonly IMemoryCache _cache;

        public EmployeeCommonController(IRequestService requestService, ClaimHelper claimHelper, IMemoryCache cache, 
            IUserTypeService userTypeService)
        {
            _requestService = requestService;
            _claimHelper = claimHelper;
            _cache = cache;
            _userTypeService = userTypeService;
        }

        [HttpGet]
        [Authorize(Roles = "AnimalWatcher,Receptionist")]
        [ActionMapper("GetEmployeeRequests", "EmployeeCommon", "Your Requests")]
        public async Task<IActionResult> GetEmployeeRequests(int? pageIndex, long? activeRequestId)
        {
            int index = pageIndex ?? 1, pageSize = 10;
            long employeeId = Convert.ToInt64(_claimHelper.GetClaimValue(ClaimTypes.Sid));

            var userTask = UserViewModel.CreateUser(_claimHelper, _userTypeService, _cache);
            var requestsTask = _requestService.GetRequestsByPageIndex(employeeId, index, pageSize);


            EmployeeBaseViewModel employee = new(await userTask)
            {
                ActiveAction = "GetEmployeeRequests",
            };

            if (activeRequestId != null)
            {
                employee.IsInteractedWithModal = true;
                employee.ActiveRequest = (await _requestService.GetRequestById(activeRequestId))!;
            }

            var requests = await requestsTask;
            int allEmployeeRequests = await _requestService.GetEmployeeRequestsCount(employeeId);

            employee.Requests = new PaginatedList<Request>(requests, allEmployeeRequests, index, pageSize);

            return View("Requests", employee);
        }

        [Authorize(Roles = "AnimalWatcher,Receptionist")]
        public async Task<IActionResult> DeleteEmployeeRequest(long requestId, string status)
        {
            string statusAbleToDelete = "Sent";
            if (string.Compare(status, statusAbleToDelete, StringComparison.OrdinalIgnoreCase) == 0)
            {
                await _requestService.DeleteRequest(requestId);
            }

            return RedirectToAction("GetEmployeeRequests");
        }

        [HttpGet]
        [Authorize(Roles = "AnimalWatcher,Receptionist")]
        public async Task<IActionResult> EditRequestView(long requestId, long pageIndex)
        {
            var user = await UserViewModel.CreateUser(_claimHelper, _userTypeService, _cache);
            var request = await _requestService.GetRequestById(requestId);
            TempData["r_pageIndex"] = pageIndex.ToString();

            EmployeeBaseViewModel employee = new(user)
            {
                ActiveRequest = request!,
            };

            return View("EditRequest", employee);
        }

        [HttpPost]
        [Authorize(Roles = "AnimalWatcher,Receptionist")]
        public async Task<IActionResult> UpdateEmployeeRequest(EmployeeBaseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                long requestId = model.ActiveRequest!.Id;
                model = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _cache));
                model.ActiveRequest = await _requestService.GetRequestById(requestId);

                return View("EditRequest", model);
            }
            
            await _requestService.UpdateRequest(model.ActiveRequest!);

            long pageIndex = Convert.ToInt64(TempData["r_pageIndex"]!);

            return RedirectToAction("GetEmployeeRequests", new {pageIndex});
        }

        [HttpGet]
        [Authorize(Roles = "AnimalWatcher,Receptionist")]
        public async Task<IActionResult> AddRequestView(long pageIndex)
        {
            EmployeeBaseViewModel employee = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _cache))
            {
                ActiveRequest = new Request()
                {
                    Status = await _requestService.GetRequestStatusByStatus("Sent"),
                }
            };

            if (employee.ActiveRequest!.Status == null)
            {
                Response.StatusCode = 501;
            }
            TempData["a_pageIndex"] = pageIndex.ToString();

            return View("AddRequest", employee);
        }

        [HttpPost]
        [Authorize(Roles = "AnimalWatcher,Receptionist")]
        public async Task<IActionResult> AddNewRequest(EmployeeBaseViewModel model)
        {
            Request addedRequest = model.ActiveRequest!;
            addedRequest.Status = await _requestService.GetRequestStatusByStatus("Sent");
            if (!ModelState.IsValid)
            {
                model = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _cache));
                model.ActiveRequest = addedRequest;
                return View("AddRequest", model);
            }

            addedRequest.EmployeeId = Convert.ToInt64(_claimHelper.GetClaimValue(ClaimTypes.Sid));
            addedRequest.StatusId = addedRequest.Status!.Id;

            await _requestService.AddRequest(addedRequest);

            long pageIndex = Convert.ToInt64(TempData["a_pageIndex"]!);

            return RedirectToAction("GetEmployeeRequests", new { pageIndex });
        }

        [HttpGet]
        [Authorize(Roles = "AnimalWatcher")]
        public async Task<IActionResult> GetWatcherRooms()
        {
            throw new NotImplementedException();
        }
    }
}
