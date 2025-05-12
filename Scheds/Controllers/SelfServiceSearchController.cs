using Microsoft.AspNetCore.Mvc;

namespace Scheds.MVC.Controllers
{
    public class SelfServiceSearchController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
