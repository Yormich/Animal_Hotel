using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Models.ViewModels.RoleViewModels;
using Animal_Hotel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
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
        private readonly IIFileProvider _fileProvider;
        private readonly IContractService _contractService;
        private readonly IReviewService _reviewService;

        public ClientController(IAnimalService animalService, ClaimHelper claimHelper, IUserTypeService userTypeService,
            IMemoryCache memoryCache, IIFileProvider fileProvider, IContractService contractService,
            IReviewService reviewService) 
        {
            _animalService = animalService;
            _claimHelper = claimHelper;
            _userTypeService = userTypeService;
            _memoryCache = memoryCache;
            _fileProvider = fileProvider;
            _contractService = contractService;
            _reviewService = reviewService;
        }

        [HttpGet]
        [Authorize(Roles = "Client")]
        [ActionMapper("GetClientAnimals", "Client", "Animals")]
        public async Task<IActionResult> GetClientAnimals(int? pageIndex, long? animalId)
        {
            long clientId = Convert.ToInt64(_claimHelper.GetClaimValue(ClaimTypes.Sid));
            int index = pageIndex ?? 1, pageSize = 6;
            var clientAnimals = await _animalService.GetClientAnimals(clientId, index, pageSize);
            var allClientAnimalsCount = await _animalService.GetClientAnimalsCount(clientId);

            ClientDataViewModel client = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache)) 
            {
                Animals = new PaginatedList<Animal>(clientAnimals, allClientAnimalsCount, index, pageSize),
                ActiveAction = "GetClientAnimals",
            };

            if(animalId != null)
            {
                client.ActiveAnimal = (await _animalService.GetAnimalById(animalId))!;
                client.IsInteractedWithModal = true;
            }

            return View("Animals", client);
        }

        [HttpGet]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> EditAnimalView(long animalId, int pageIndex)
        {
            ClientDataViewModel client = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache))
            {
                ActiveAnimal = await _animalService.GetAnimalById(animalId),
            };

            TempData["animal_e_pageIndex"] = pageIndex;

            return View("EditAnimal", client);
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> UpdateAnimal(ClientDataViewModel client, string oldPhotoPath)
        {
            var editedAnimal = client.ActiveAnimal!;
            editedAnimal.PhotoPath = oldPhotoPath;

            if (Request.Form.Files.Count != 0 && ModelState.IsValid) 
            {
                IFormFile file = Request.Form.Files[0];

                if (_fileProvider.IsFileExtensionSupported(file.FileName))
                {
                    string uniqueFilePath = $"{client.Login}_animal{editedAnimal.Id}_@fileName";
                    await _fileProvider.RemoveFileFromServer(uniqueFilePath.Replace("@fileName", oldPhotoPath));
                    editedAnimal.PhotoPath = file.FileName;
                    await _fileProvider.UploadFileToServer(file, uniqueFilePath.Replace("@fileName", editedAnimal.PhotoPath));
                }
                else
                {
                    ModelState.AddModelError<ClientDataViewModel>(c => c.ActiveAnimal!.PhotoPath, 
                        "This file extension is not supported.");
                }
            }

            if (!ModelState.IsValid)
            {
                editedAnimal.AnimalType = await _animalService.GetAnimalTypeById(editedAnimal.TypeId);
                client = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache))
                {
                    ActiveAnimal = editedAnimal
                };

                return View("EditAnimal", client);
            }

            await _animalService.UpdateAnimal(editedAnimal);
            int pageIndex = (int)TempData["animal_e_pageIndex"]!;

            return RedirectToAction("GetClientAnimals", new { pageIndex });
        }

        [HttpGet]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> DeleteAnimal(long animalId, int pageIndex)
        {
            if (await _animalService.AnimalHasActiveContractOrBooking(animalId))
            {
                TempData["DeleteError"] = "You can't delete animal with active contract or booking.";
                return RedirectToAction("GetClientAnimals", new { pageIndex, animalId });
            }

            await _animalService.DeleteAnimalById(animalId);

            return RedirectToAction("GetClientAnimals", new { pageIndex });
        }

        [HttpGet]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> AddAnimalView(int pageIndex)
        {
            ClientDataViewModel client = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache))
            {
                ActiveAnimal = new(),
                AnimalTypes = await _animalService.GetAnimalTypes(),
            };

            TempData["animal_a_pageIndex"] = pageIndex;

            return View("AddAnimal", client);
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> AddAnimal(ClientDataViewModel model)
        {
            var animal = model.ActiveAnimal!;
            ClientDataViewModel client = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache));
            IFormFile? file = null;

            if (Request.Form.Files.Count != 0 && ModelState.IsValid)
            {
                file = Request.Form.Files[0];
                if (_fileProvider.IsFileExtensionSupported(file.FileName))
                {
                    animal.PhotoPath = file.FileName;
                }
                else
                {
                    ModelState.AddModelError<ClientDataViewModel>(c => c.ActiveAnimal!.PhotoPath, 
                        "This file extension is not supported.");
                }
            }
            else
            {
                var choosenAnimalType = (await _animalService.GetAnimalTypeById(animal.TypeId)).Name;
                animal.PhotoPath = $"Default{choosenAnimalType}.png";
            }

            if (!ModelState.IsValid)
            {
                client.ActiveAnimal = animal;
                client.AnimalTypes = await _animalService.GetAnimalTypes();

                return View("AddAnimal", client);
            }

            
            long createdAnimalId = await _animalService.CreateAnimal(animal);
            if (file != null)
            {
                await _fileProvider.UploadFileToServer(file!, $"{client.Login}_animal{createdAnimalId}_{file!.FileName}");
            }

            int? pageIndex = (int?)TempData["animal_a_pageIndex"];

            return RedirectToAction("GetClientAnimals", new { pageIndex });
        }


        [HttpGet]
        [Authorize(Roles = "Client")]
        [ActionMapper("ClientReview", "Client", "Hotel Review")]
        public async Task<IActionResult> ClientReview()
        {
            long clientId = Convert.ToInt64(_claimHelper.GetClaimValue(ClaimTypes.Sid));

            ClientDataViewModel client = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache)) 
            {
                HotelReview = await _reviewService.GetClientReview(clientId),
                HasFinishedContracts = await _contractService.DoesClientHasFinishedContract(clientId), 
                ActiveAction = "ClientReview",
            };

            return View("ClientReview", client);
        }

        [HttpGet]
        [Authorize(Roles = "Client")]
        public async Task<ViewResult> ReviewCreationPage()
        {
            ClientDataViewModel client = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache));
            client.HotelReview = new Review()
            {
                ClientId = client.SubUserId,
            };

            return View("AddReview", client);
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> AddReview(ClientDataViewModel model)
        {
            Review clientReview = model.HotelReview!;
            model = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache));

            if (!ModelState.IsValid)
            {
                model.HotelReview = clientReview;
                return View("AddReview", model);
            }

            var procedureRes = await _reviewService.CreateReview(clientReview);

            if (!procedureRes.Item1)
            {
                model.HotelReview = clientReview;
                TempData["addReviewErr"] = procedureRes.message;
                return View("AddReview", model);
            }

            return RedirectToAction("ClientReview");
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> UpdateClientReview(ClientDataViewModel model)
        {
            Review clientReview = model.HotelReview!;
            model = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache));

            if (!ModelState.IsValid)
            {
                model.HotelReview = clientReview;
                return View("ClientReview", model);
            }

            await _reviewService.UpdateReview(clientReview);

            return RedirectToAction("ClientReview");
        }

        [HttpGet]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> DeleteClientReview(long reviewId)
        {
            await _reviewService.DeleteReview(reviewId);

            return RedirectToAction("ClientReview");
        }
    }
}
