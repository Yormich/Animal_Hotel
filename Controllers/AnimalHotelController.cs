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
using Animal_Hotel.Services;

namespace Animal_Hotel.Controllers
{
    public class AnimalHotelController : Controller
    {
        private readonly ILogger<AnimalHotelController> _logger;
        private readonly IReviewService _reviewService;
        public AnimalHotelController(IReviewService reviewService, ILogger<AnimalHotelController> logger)
        {
            _logger = logger;
            _reviewService = reviewService;
        }
        public IActionResult Index()
        {
            HomePageViewModel model = new HomePageViewModel();
            return View(model);
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