using Microsoft.AspNetCore.Mvc;

namespace Scheds.MVC.Controllers
{
    public class GenerateSchedulesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}