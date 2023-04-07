using Animal_Hotel.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using Animal_Hotel.Models.ViewModels;
using Animal_Hotel.Models.DatabaseModels;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Animal_Hotel.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AnimalHotelDbContext _db;
        private readonly IConfiguration _configuration;
        public HomeController(ILogger<HomeController> logger, AnimalHotelDbContext db, IConfiguration config)
        {
            _logger = logger;
            _db = db;
            _configuration = config;
        }

        [HttpGet("Home/TestContext")]
        public IActionResult TestContext()
        {
            _db.UserTypes.Load();
            _db.LoginInfos.Load();
            StringBuilder sb = new();
            foreach(var ut in _db.UserTypes)
            {
                sb.AppendLine($"User Type: {ut.Id} {ut.Name}");
                foreach(var userClient in ut.UserLoginInfos)
                {
                    sb.AppendLine($"\tClient Info: {userClient.Email} {userClient.PhoneNumber}");
                }
                
            }
            return new JsonResult(sb.ToString());
        }


        [HttpGet("Home/Login/{id?}")]
        public IActionResult Login([FromRoute] int? id)
        {
            if (id is null)
            {
                return StatusCode(403);
            }

            Dictionary<int, string> roles = new()
            {
                {1, "HotelManager"},
                {2, "Receptionist"},
                {3, "AnimalWatcher"},
                { 4, "Client"}
            };

            string role = roles[id ?? 1];

            List<Claim> claims = new() { new Claim(ClaimTypes.Role, role) };
            var token = new JwtSecurityToken
                (
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(8),
                    signingCredentials: new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
                        SecurityAlgorithms.HmacSha256)
                );
            var handler = new JwtSecurityTokenHandler();
            string encodedJwt = handler.WriteToken(token);
            HttpContext.Session.SetString("accessToken", encodedJwt);

            return new JsonResult(new
            {
                status = Ok(),
                token = encodedJwt
            });
        } 

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}