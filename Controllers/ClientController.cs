using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Models.ViewModels.RoleViewModels;
using Animal_Hotel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace Animal_Hotel.Controllers
{
    public class ClientController : Controller
    {
        private readonly IAnimalService _animalService;
        private readonly ClaimHelper _claimHelper;
        private readonly IUserTypeService _userTypeService;
        private readonly IMemoryCache _memoryCache;

        public ClientController(IAnimalService animalService, ClaimHelper claimHelper, IUserTypeService userTypeService,
            IMemoryCache memoryCache) 
        {
            _animalService = animalService;
            _claimHelper = claimHelper;
            _userTypeService = userTypeService;
            _memoryCache = memoryCache;
        }

        [HttpGet]
        [Authorize(Roles = "Client")]
        [ActionMapper("GetClientAnimals", "Client", "Your Animals")]
        public async Task<IActionResult> GetClientAnimals(int? pageIndex)
        {
            long clientId = Convert.ToInt64(_claimHelper.GetClaimValue(ClaimTypes.Sid));
            int index = pageIndex ?? 1, pageSize = 6;
            var animalsTask = _animalService.GetClientAnimals(clientId, index, pageSize);

            //TODO:rebuild
            ClientDataViewModel client = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache)) 
            {
                Animals = new PaginatedList<Animal>(await animalsTask, 12, index, pageSize),
            };

            return View("GetClientAnimals", client);
        }

        [HttpGet]
        public async Task<IActionResult> EditAnimalView()
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAnimal()
        {
            throw new NotImplementedException();

        }

        [HttpPost]
        public async Task<IActionResult> DeleteAnimal()
        {
            throw new NotImplementedException();

        }

        [HttpGet]
        public async Task<IActionResult> AddAnimalView()
        {
            throw new NotImplementedException();

        }

        [HttpPost]
        public async Task<IActionResult> AddAnimal()
        {
            throw new NotImplementedException();

        }
    }
}
