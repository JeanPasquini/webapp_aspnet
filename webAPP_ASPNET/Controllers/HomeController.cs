using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using webAPP_ASPNET.Models;
using YourNamespace.Filters;

namespace webAPP_ASPNET.Controllers
{
    [TokenAuthorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("ErrorMessage") != null)
            {
                TempData["ErrorMessage"] = HttpContext.Session.GetString("ErrorMessage");
                HttpContext.Session.Remove("ErrorMessage");
            }
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
