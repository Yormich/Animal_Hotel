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
#pragma warning disable IDE0090



namespace Animal_Hotel.Controllers
{
    public class AnimalHotelController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly IRoomService _roomService;

        public AnimalHotelController(IReviewService reviewService, IRoomService roomService)
        {
            _reviewService = reviewService;
            _roomService = roomService;
        }
        public async Task<IActionResult> Index()
        {
            HomePageViewModel model = new HomePageViewModel()
            {
                ToView = "Index",
                Reviews = await _reviewService.GetLastReviews(5),
            };

            return View(model);
        }

        public async Task<ViewResult> Rooms(int? pageIndex, bool? isNotAuthorized)
        {
            int index = pageIndex ?? 1, pageSize = 5;
            var rooms = await _roomService.GetRoomsByPageIndex(index, pageSize);
            HomePageViewModel model = new HomePageViewModel()
            {
                ToView = "Rooms",
                Rooms = new PaginatedList<Room>(rooms, await _roomService.GetRoomsCountAsync(), index, pageSize),
                IsInteractedWithModal = isNotAuthorized ?? false
            };
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