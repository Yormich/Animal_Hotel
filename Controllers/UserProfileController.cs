using Animal_Hotel.Services;
using Microsoft.AspNetCore.Mvc;

namespace Animal_Hotel.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly ClaimHelper claimHelper;

        public UserProfileController(ClaimHelper claimHelper)
        {
            this.claimHelper = claimHelper;
        }

        public IActionResult UserProfile(int userId)
        {
            return View();
        }
    }
}
