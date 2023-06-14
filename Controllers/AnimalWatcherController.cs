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

        public AnimalWatcherController(ClaimHelper claimHelper, IUserTypeService userTypeService, IMemoryCache memoryCache, 
            IRoomService roomService, IAnimalService animalService)
        {
            _claimHelper = claimHelper;
            _userTypeService = userTypeService;
            _memoryCache = memoryCache;
            _roomService = roomService;
            _animalService = animalService;
        }

        [HttpGet]
        [Authorize(Roles = "AnimalWatcher")]
        [ActionMapper("GetWatcherRelatedRooms", "AnimalWatcher", "Your Rooms")]
        public async Task<IActionResult> GetWatcherRelatedRooms()
        {
            AnimalWatcherViewModel watcher = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache)) 
            {
                ActiveAction = "GetWatcherRelatedRooms",
            };
            watcher.Rooms = await _roomService.GetRoomsWithFreeEnclosuresCount(watcher.SubUserId);
            
            return View("WatcherRooms", watcher);
        }

        [HttpGet]
        [Authorize(Roles = "AnimalWatcher")]
        [ActionMapper("GetWatcherAnimals", "AnimalWatcher", "Animals from Rooms")]
        public async Task<IActionResult> GetWatcherAnimals(int? pageIndex, long? animalId)
        {
            int index = pageIndex ?? 1, pageSize = 12;
            AnimalWatcherViewModel watcher = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache)) 
            {
                ActiveAction = "GetWatcherAnimals",
            };
            var watcherAnimals = await _animalService.GetAnimalsByWatcherId(watcher.SubUserId);
            watcher.Animals = new(watcherAnimals.Skip((index - 1) * pageSize).Take(pageSize), watcherAnimals.Count, index, pageSize);

            if (animalId != null)
            {
                watcher.IsInteractedWithModal = true;
                watcher.ActiveAnimal = await _animalService.GetAnimalById(animalId);
            }

            return View("WatcherAnimals", watcher);
        }
    }
}
