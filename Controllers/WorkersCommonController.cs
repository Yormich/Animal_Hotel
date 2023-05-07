using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Animal_Hotel.Controllers
{
    public class WorkersCommonController : Controller
    {
        [HttpGet]
        [Authorize(Roles = "AnimalWatcher,Receptionist")]
        [ActionMapper("GetWorkerRequests", "WorkersCommon", "Requests")]
        public async Task<IActionResult> GetWorkerRequests(long workerId)
        {
            return View("", "ASD");
        }
    }
}
