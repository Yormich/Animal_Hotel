using Animal_Hotel.Models.ViewModels.RoleViewModels;
using Animal_Hotel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Animal_Hotel.Controllers
{
    public class AnimalWatcherController : Controller
    {
        private readonly ClaimHelper _claimHelper;
        public AnimalWatcherController(ClaimHelper claimHelper)
        {
            _claimHelper = claimHelper;
        }

        [HttpGet]
        [Authorize(Roles = "AnimalWatcher")]
        [ActionMapper("GetWatcherRelatedRooms", "HotelRooms", "Your Rooms")]
        public async Task<IActionResult> GetWatcherRelatedRooms()
        {
            long employeeId = Convert.ToInt64(_claimHelper.GetClaimValue(ClaimTypes.Sid));
            var userTask = _claimHelper.FormUserByClaims();


            AnimalWatcherViewModel animalWatcher = new(await userTask);
            return View("WatcherRooms", animalWatcher);
        }
    }
}
