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
        private readonly IUserRegisterService _userRegisterService;
        private readonly IIFileProvider _fileProvider;
        private readonly IReviewService _reviewService;
        private readonly IRoomService _roomService;

        public LoginAndRegisterController(IUserLoginInfoService userLoginsRepository, IUserRegisterService userRegisterService,
            IConfiguration configuration, IIFileProvider fileProvider, IReviewService reviewService, IRoomService roomService)
        {
            _configuration = configuration;
            _userLoginInfoRepository = userLoginsRepository;
            _userRegisterService = userRegisterService;
            _fileProvider = fileProvider;
            _reviewService = reviewService;
            _roomService = roomService;
        }

        [HttpGet("Register")]
        public IActionResult Register()
        {
            return View(new ClientRegisterModel());
        }

        [HttpPost("/Register")]
        public async Task<IActionResult> ConfirmClientRegistration(ClientRegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Register", model);
            }

            if ((DateTime.Now.Subtract(model.BirthDate).Days / 365 ) < 16)
            {
                ModelState.AddModelError("BirthDate", "You should be older 16 to create an account");
                return View("Register", model);
            }

            var files = HttpContext.Request.Form.Files;
            if(files.Any())
            {
                var file = files[0];
                if (_fileProvider.IsFileExtensiionSupported(file.FileName))
                {
                    model.PhotoPath = file.FileName;
                    await _fileProvider.UploadFileToServer(file, $"{model.Login}_{file.FileName}");
                }
                else
                {
                    ModelState.AddModelError("PhotoPath", "This file extension is not supported");
                    return View("Register", model);
                }
            }

            Response.StatusCode = await _userRegisterService.RegisterClient(model) ? 200 : 500;

            return RedirectToAction("Index", "AnimalHotel");
        }

        [HttpPost("ConfirmLogin")]
        public async Task<IActionResult> Login([FromForm] HomePageViewModel model, int? pageIndex, int? pageSize)
        {
            model.IsTriedToLogin = true;
            await RecoverModel(model, pageIndex, pageSize);

            if (!ModelState.IsValid)
            {
                return View(model.ToView, model);
            }

            var userLogin = await _userLoginInfoRepository.GetLoginWithRoleAndPersonalInfo(model.Login);

            return HandleLoginResult(model, userLogin);
        }

        private IActionResult HandleLoginResult(HomePageViewModel model, UserLoginInfo? userLogin)
        {
            Response.StatusCode = 401;

            if (userLogin == null)
            {
                ModelState.AddModelError("Login", "There isn't any user with such email.");
                return View(model.ToView, model);
            }
            string hashedPassword = Sha256_Hash(model.Password);

            if (string.Compare(hashedPassword, new StringBuilder().GetString(userLogin.Password), true) != 0)
            {
                ModelState.AddModelError("Password", "Wrong password, please try again");
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

        private static string Sha256_Hash(string value)
        {
            var sb = new StringBuilder();
            using (SHA256 hash = SHA256.Create())
            {
                Byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(value));
                return sb.GetString(result);
            }
        }

        private string CreateToken(UserLoginInfo user)
        {
            string? firstName = user.Client?.FirstName ?? user.Employee?.FirstName;
            string? lastName = user.Client?.LastName ?? user.Employee?.LastName;
            string? photoPath = user.Client?.PhotoPath ?? user.Employee?.PhotoPath;

            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Sid, Convert.ToString(user.Id)),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.UserType.Name),
                new Claim(ClaimTypes.Name, $"{firstName} {lastName}"),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                new Claim("ProfileImagePath", photoPath ?? "UnsetClient.png")
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
