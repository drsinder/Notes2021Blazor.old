using Microsoft.AspNetCore.Mvc;

namespace Notes2021.Controllers
{
    public class NavigationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Screens()
        {
            return View();
        }
    }
}