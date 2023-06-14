using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Models.ViewModels.RoleViewModels;
using Animal_Hotel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using System.Diagnostics.Eventing.Reader;

namespace Animal_Hotel.Controllers
{
    public class ReceptionistController : Controller
    {
        private readonly IUserTypeService _userTypeService;
        private readonly ClaimHelper _claimHelper;
        private readonly IMemoryCache _memoryCache;
        private readonly IContractService _contractService;
        private readonly IBookingService _bookingService;
        private readonly IAnimalService _animalService;
        private readonly IClientService _clientService;
        private readonly IIFileProvider _fileProvider;
        private readonly IImagePathProvider _pathProvider;
        private readonly IEnclosureService _enclosureService;

        public ReceptionistController(IUserTypeService userTypeService, ClaimHelper claimHelper, IMemoryCache memoryCache, 
            IContractService contractService, IBookingService bookingService, IAnimalService animalService,
            IClientService clientService, IIFileProvider fileProvider, IImagePathProvider pathProvider,
            IEnclosureService enclosureService)
        {
            _userTypeService = userTypeService;
            _claimHelper = claimHelper;
            _memoryCache = memoryCache;
            _contractService = contractService;
            _bookingService = bookingService;
            _animalService = animalService;
            _clientService = clientService;
            _fileProvider = fileProvider;
            _pathProvider = pathProvider;
            _enclosureService = enclosureService;
        }
        public Task<IActionResult> Reports()
        {
            throw new NotImplementedException();

        }

        [HttpGet]
        [Authorize(Roles = "Receptionist")]
        [ActionMapper("Bookings", "Receptionist", "Bookings")]
        public async Task<IActionResult> Bookings(DateTime? targetDate)
        {
            DateTime target = targetDate ?? DateTime.Now;
            ReceptionistViewModel receptionist = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache))
            {
                TargetDate = target,
                ActiveAction = "Bookings",
                BaseReports = await _bookingService.GetBookingsByDate(target),
            };

            return View("Bookings", receptionist);
        }

        [HttpGet]
        [Authorize(Roles = "Receptionist")]
        [ActionMapper("GetContracts", "Receptionist", "Contracts")]
        public async Task<IActionResult> GetContracts(DateTime? targetDate)
        {
            DateTime target = targetDate ?? DateTime.Now;
            ReceptionistViewModel receptionist = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache))
            {
                ActiveAction = "GetContracts",
                BaseReports = await _contractService.GetContractsByDate(target),
                TargetDate = target,
            };
            return View("Contracts", receptionist);
        }

        [HttpGet]
        [Authorize(Roles = "Receptionist")]
        public async Task<IActionResult> DeleteContract(long contractId, long targetOffset)
        {
            DateTime targetDate = DateTimeOffset.FromUnixTimeSeconds(targetOffset).DateTime;

           var result = await _contractService.DeleteContract(contractId);

            if (!result.success)
            {
                TempData["cDeleteError"] = result.message;
                ReceptionistViewModel receptionist = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache))
                {
                    ActiveAction = "GetContracts",
                    BaseReports = await _contractService.GetContractsByDate(targetDate),
                    TargetDate = targetDate,
                };
                return View("Contracts", receptionist);
            }

            return RedirectToAction("GetContracts");
        }

        public async Task<IActionResult> DeleteBooking(long animalId, long startOffset, long targetOffset)
        {
            DateTime startDate = DateTimeOffset.FromUnixTimeSeconds(startOffset).DateTime, 
                targetDate = DateTimeOffset.FromUnixTimeSeconds(targetOffset).DateTime;
            if (startDate >= DateTime.Now)
            {
                TempData["dBookingError"] = "You can't delete bookings that are not expired yet.";
                return RedirectToAction("Bookings", "Receptionist", new {targetDate});
            }

            await _bookingService.DeleteBooking(animalId);

            return RedirectToAction("Bookings");
        }

        [HttpGet]
        [Authorize(Roles = "Receptionist")]
        [ActionMapper("Clients", "Receptionist", "Hotel Clients")]
        public async Task<IActionResult> Clients(int? pageIndex, long? clientId)
        {
            int index = pageIndex ?? 1, pageSize = 12;
            ReceptionistViewModel receptionist = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache))
            {
                ActiveAction = "Clients",
                Clients = new(await _clientService.GetClientsByPageIndex(index, pageSize),
                await _clientService.GetClientCount(), index, pageSize),
            };

            var clients = await _clientService.GetClientsByPageIndex(index, pageSize);

            if (clientId != null)
            {
                receptionist.IsInteractedWithModal = true;
                receptionist.ActiveClient = await _clientService.GetClientById(clientId);
            }

            return View("Clients", receptionist);
        }

        [HttpGet]
        [Authorize(Roles = "Receptionist")]
        public async Task<IActionResult> AddClientAnimalView(long clientId)
        {
            ReceptionistViewModel receptionist = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache))
            {
                ActiveAnimal = new()
                {
                    OwnerId = clientId,
                },
                AnimalTypes = new(await _animalService.GetAnimalTypes()),
                ActiveClient = await _clientService.GetClientById(clientId)
            };
            return View("AddAnimal", receptionist);
        }

        [HttpPost]
        [Authorize(Roles = "Receptionist")]
        public async Task<IActionResult> AddClientAnimal(ReceptionistViewModel model)
        {
            var animal = model.ActiveAnimal!;
            var owner = await _clientService.GetClientById(animal.OwnerId);
            ReceptionistViewModel receptionist = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache));
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
                foreach(var p in ModelState)
                {
                    Console.WriteLine(p.Key);
                    foreach(var e in p.Value.Errors)
                    {
                        Console.WriteLine($"\t{e.ErrorMessage}");
                    }
                }
                receptionist.ActiveAnimal = animal;
                receptionist.AnimalTypes = new(await _animalService.GetAnimalTypes());
                receptionist.ActiveClient = owner;

                return View("AddAnimal", receptionist);
            }


            long createdAnimalId = await _animalService.CreateAnimal(animal);
            if (file != null)
            {
                await _fileProvider.UploadFileToServer(file!,
                    _pathProvider.BuildAnimalFileName(owner!.LoginInfo!.Email, createdAnimalId, file!.FileName));
            }

            return RedirectToAction("Clients");
        }


        [HttpGet]
        [Authorize(Roles = "Receptionist")]
        public async Task<IActionResult> EditClientAnimalView(long animalId)
        {
            ReceptionistViewModel receptionist = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache))
            {
                ActiveAnimal = await _animalService.GetAnimalById(animalId),

            };

            return View("EditAnimal", receptionist);
        }

        [HttpPost]
        [Authorize(Roles = "Receptionist")]
        public async Task<IActionResult> EditClientAnimal(ReceptionistViewModel receptionist, string oldPhotoPath)
        {
            var editedAnimal = receptionist.ActiveAnimal!;
            var owner = await _clientService.GetClientById(editedAnimal.OwnerId);   
            editedAnimal.PhotoPath = oldPhotoPath;

            if (Request.Form.Files.Count != 0 && ModelState.IsValid)
            {
                IFormFile file = Request.Form.Files[0];

                if (_fileProvider.IsFileExtensionSupported(file.FileName))
                {
                    await _fileProvider.RemoveFileFromServer(_pathProvider.BuildAnimalFileName(owner!.LoginInfo!.Email, editedAnimal.Id, oldPhotoPath));
                    editedAnimal.PhotoPath = file.FileName;
                    await _fileProvider.UploadFileToServer(file,
                        _pathProvider.BuildAnimalFileName(owner!.LoginInfo!.Email, editedAnimal.Id, editedAnimal.PhotoPath));
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
                receptionist = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache))
                {
                    ActiveAnimal = editedAnimal
                };

                return View("EditAnimal", receptionist);
            }

            await _animalService.UpdateAnimal(editedAnimal);

            return RedirectToAction("Clients");
        }

        [HttpGet]
        [Authorize(Roles = "Receptionist")]
        public async Task<IActionResult> DeleteClientAnimal(int pageIndex, long clientId, long animalId)
        {
            var result = await _animalService.DeleteAnimalById(animalId);

            if (!result.success)
            {
                TempData["dAnimalError"] = result.message;
            }

            return RedirectToAction("Clients", new { pageIndex, clientId });
        }

        [HttpGet]
        [Authorize(Roles = "Receptionist")]
        public async Task<IActionResult> CreateClientContract(long clientId)
        {
            ReceptionistViewModel receptionist = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache))
            {
                Animals = await _animalService.GetAvailableClientAnimals(clientId),
            };

            return View("SelectClientAnimal", receptionist);
        }


        [HttpGet]
        [Authorize(Roles = "Receptionist")]
        public async Task<IActionResult> ContractCreation(ReceptionistViewModel model)
        {
            ReceptionistViewModel receptionist = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache)) 
            {
                SelectedAnimalId = model.SelectedAnimalId,
            };

            if (model.SelectedAnimalId == default)
            {
                ModelState.AddModelError("SelectedAnimalId", "Selected animal is required for contact creation");
            }

            if (!ModelState.IsValid)
            {
                model = receptionist;
                return View("SelectClientAnimal", model);
            }

            receptionist.Enclosures = await _enclosureService.GetSuitableEnclosures(receptionist.SelectedAnimalId);
            receptionist.ActiveAnimal = await _animalService.GetAnimalById(receptionist.SelectedAnimalId);
            receptionist.ActiveContract = new()
            {
                AnimalId = receptionist.SelectedAnimalId,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(7),
            };

            return View("AddContract", receptionist);
        }

        [HttpPost]
        [Authorize(Roles = "Receptionist")]
        public async Task<IActionResult> ConfirmContractCreation(ReceptionistViewModel receptionist)
        {
            var activeContract = receptionist.ActiveContract!;
            if (activeContract.StartDate < DateTime.Now)
            {
                ModelState.AddModelError("ActiveContract.StartDate", "Contract date can not be in past");
            }

            if (activeContract.EndDate.Subtract(activeContract.StartDate).Days < 1)
            {
                ModelState.AddModelError("ActiveContract.EndDate", "Contract end date must be at least 1 day greater than start date");
            }

            if (activeContract.EnclosureId == default)
            {
                ModelState.AddModelError("ActiveContract.EnclosureId", "Enclosure must be selected for contract creation.");
            }

            if (!ModelState.IsValid)
            {
                foreach(var p in ModelState)
                {
                    Console.WriteLine(p.Key);
                    foreach(var e in p.Value.Errors)
                    {
                        Console.WriteLine($"\t{e.ErrorMessage}");
                    }
                }
                receptionist = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache));
                receptionist.Enclosures = await _enclosureService.GetSuitableEnclosures(activeContract.AnimalId);
                receptionist.ActiveAnimal = await _animalService.GetAnimalById(activeContract.AnimalId);
                receptionist.ActiveContract = activeContract;
                return View("AddContract", receptionist);
            }

            var result = await _contractService.CreateContract(activeContract);

            if (!result.success)
            {
                TempData["aContractError"] = result.message;
                receptionist = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache));
                receptionist.Enclosures = await _enclosureService.GetSuitableEnclosures(activeContract.AnimalId);
                receptionist.ActiveAnimal = await _animalService.GetAnimalById(activeContract.AnimalId);
                receptionist.ActiveContract = activeContract;
                return View("AddContract", receptionist);
            }

            return RedirectToAction("Contracts");
        }

        public async Task<IActionResult> FillContractByBooking(long animalId)
        {
            ReceptionistViewModel receptionist = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache))
            {
                Enclosures = await _enclosureService.GetSuitableEnclosures(animalId),
                ActiveAnimal = await _animalService.GetAnimalById(animalId),
                SelectedAnimalId = animalId
            };
            var booking = await _bookingService.GetBookingById(animalId);

            if (booking == null)
            {
                TempData["noBookingError"] = "There is no booking for this animal.";
                receptionist.ActiveContract = new()
                {
                    AnimalId = receptionist.SelectedAnimalId,
                    StartDate = DateTime.Now.AddDays(1),
                    EndDate = DateTime.Now.AddDays(7),
                };
                return View("AddContract", receptionist);
            }

            receptionist.ActiveContract = new()
            {
                AnimalId = booking.AnimalId,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                EnclosureId = booking.EnclosureId
            };

            return View("AddContract", receptionist);
        }

        [HttpGet]
        [Authorize(Roles = "Receptionist")]
        public async Task<IActionResult> CloseContractView(long contractId)
        {
            ReceptionistViewModel receptionist = new(await UserViewModel.CreateUser(_claimHelper, _userTypeService, _memoryCache))
            {
                ActiveContract = await _contractService.GetContractById(contractId),

            }
        }

        [HttpPost]
        [Authorize(Roles = "Receptionist")]
        public async Task<IActionResult> CloseContract(ReceptionistViewModel receptionist)
        {
            return RedirectToAction("Contracts");
        }
    }
}
