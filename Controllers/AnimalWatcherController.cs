using Animal_Hotel.Models.ViewModels.RoleViewModels;
using Animal_Hotel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace Animal_Hotel.Controllers
{
    public class AnimalWatcherController : Controller
    {
        private readonly ClaimHelper _claimHelper;
        private readonly IUserTypeService _userTypeService;
        private readonly IMemoryCache _memoryCache;
        private readonly IRoomService _roomService;
        private readonly IAnimalService _animalService;
        private readonly IEnclosureService _enclosureService;

        public AnimalWatcherController(ClaimHelper claimHelper, IUserTypeService userTypeService, IMemoryCache memoryCache, 
            IRoomService roomService, IAnimalService animalService, IEnclosureService enclosureService)
        {
            _claimHelper = claimHelper;
            _userTypeService = userTypeService;
            _memoryCache = memoryCache;
            _roomService = roomService;
            _animalService = animalService;
            _enclosureService = enclosureService;
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
