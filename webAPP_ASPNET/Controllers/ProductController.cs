using Microsoft.AspNetCore.Mvc;
using YourNamespace.Filters;

namespace webAPP_ASPNET.Controllers
{
    [TypeFilter(typeof(ButtonAuthorize), Arguments = new object[] { 1 })]
    [TokenAuthorize]
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
