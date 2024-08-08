using Microsoft.AspNetCore.Mvc;

namespace DeskBookingSystem.Controllers
{
    public class Home : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
