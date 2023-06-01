using Animal_Hotel.Models.DatabaseModels;
using Animal_Hotel.Models.ViewModels;
using Animal_Hotel.Models.ViewModels.RegisterViewModels;
using Animal_Hotel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Animal_Hotel.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("Login")]
    public class LoginAndRegisterController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IUserLoginInfoService _userLoginInfoRepository;
        private readonly IClientService _clientService;
        private readonly IIFileProvider _fileProvider;
        private readonly IReviewService _reviewService;
        private readonly IRoomService _roomService;

        public LoginAndRegisterController(IUserLoginInfoService userLoginsRepository, IClientService clientService,
            IConfiguration configuration, IIFileProvider fileProvider, IReviewService reviewService, IRoomService roomService)
        {
            _configuration = configuration;
            _userLoginInfoRepository = userLoginsRepository;
            _fileProvider = fileProvider;
            _reviewService = reviewService;
            _roomService = roomService;
            _clientService = clientService;
        }

        [HttpGet("Register")]
        public IActionResult Register()
        {
            return View(new ClientRegisterModel());
        }

        [HttpPost("/Register")]
        public async Task<IActionResult> ConfirmClientRegistration(ClientRegisterModel model)
        {
            var isUserDataUnique = await _userLoginInfoRepository.IsUserWithPhoneAndEmailExists(model.Login, model.PhoneNumber);

            if (isUserDataUnique.isExists)
            {
                ModelState.AddModelError("PhoneNumber", isUserDataUnique.errorMessage);
                ModelState.AddModelError("Login", isUserDataUnique.errorMessage);
            }

            if (!UtilFuncs.IsUserOldEnough(model.BirthDate, 16))
            {
                ModelState.AddModelError("BirthDate", "Client should be older than 16 to register");
            }

            if (!ModelState.IsValid)
            {
                return View("Register", model);
            }

            var files = HttpContext.Request.Form.Files;
            if(files.Any())
            {
                var file = files[0];
                if (_fileProvider.IsFileExtensionSupported(file.FileName))
                {
                    model.PhotoPath = file.FileName;
                    await _fileProvider.UploadFileToServer(file, $"{model.Login}_{file.FileName}");
                }
                else
                {
                    ModelState.AddModelError("PhotoPath", "This file extension is not supported");
                }
            }

            if (!ModelState.IsValid)
            {
                return View("Register", model);
            }

            Response.StatusCode = await _clientService.RegisterClient(model) ? 200 : 500;

            return RedirectToAction("Index", "AnimalHotel");
        }

        [HttpPost("ConfirmLogin")]
        public async Task<IActionResult> Login([FromForm] HomePageViewModel model, int? pageIndex, int? pageSize)
        {
            model.IsInteractedWithModal = true;
            await RecoverModel(model, pageIndex, pageSize);

            if (!ModelState.IsValid)
            {
                return View(model.ToView, model);
            }

            var userLogin = await _userLoginInfoRepository.GetLoginWithRoleAndPersonalInfo(model.Login);

            if (userLogin == null)
            {
                ModelState.AddModelError("Login", "There isn't any user with such email.");
                return View(model.ToView, model);
            }

            return HandleLoginResult(model, userLogin);
        }

        private IActionResult HandleLoginResult(HomePageViewModel model, UserLoginInfo userLogin)
        {
            Response.StatusCode = 401;

            string hashedPassword = UtilFuncs.Sha256_Hash(model.Password);

            if (string.Compare(hashedPassword, new StringBuilder().GetString(userLogin.Password), true) != 0)
            {
                ModelState.AddModelError("Password", "Wrong password, please try again");
            }

            if (!ModelState.IsValid)
            {
                return View(model.ToView, model);
            }

            string token = CreateToken(userLogin);

            HttpContext.Session.SetString("access_token", token);

            return RedirectToAction(model.ToView, "AnimalHotel");
        }

        private async Task RecoverModel(HomePageViewModel model, int? pageIndex, int? pageSize)
        {
            //recover reviews
            model.Reviews = await _reviewService.GetLastReviews(5);

            //recover rooms
            int index = pageIndex ?? 1, size = pageSize ?? 5;
            var rooms = await _roomService.GetRoomsByPageIndex(index, size);
            int count = await _roomService.GetRoomsCountAsync();
            model.Rooms = new(rooms, count, index, size);
        }
        private string CreateToken(UserLoginInfo user)
        {
            string firstName = user.Client?.FirstName ?? user.Employee!.FirstName;
            string lastName = user.Client?.LastName ?? user.Employee!.LastName;
            string photoPath = user.Client?.PhotoPath ?? user.Employee!.PhotoPath;
            DateTime dateOfBirth = user.Client?.BirthDate ?? user.Employee!.BirthDate;
            long userSecondId = user.Client?.Id ?? user.Employee!.Id;

            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.PrimarySid, Convert.ToString(user.Id)),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.UserType.Name),
                new Claim(ClaimTypes.Name, $"{firstName} {lastName}"),
                new Claim(ClaimTypes.DateOfBirth, Convert.ToString(dateOfBirth)),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                new Claim(ClaimTypes.Sid, Convert.ToString(userSecondId)),
                new Claim("ProfileImagePath", photoPath ?? "UnsetClient.png"),

            };
            var token = new JwtSecurityToken
                (
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(2),
                    signingCredentials: new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
                        SecurityAlgorithms.HmacSha256)
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
