using Microsoft.AspNetCore.Mvc;

namespace Animal_Hotel.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("Login")]
    public class LoginController : Controller
    {
        public LoginController()
        {

        }

        [HttpGet]
        public IActionResult GetLoginPage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login()
        {
            return Ok();
        }
    }
}
