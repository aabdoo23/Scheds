using Microsoft.AspNetCore.Mvc;

namespace Scheds.MVC.Controllers
{
    public class SeatModerationViewController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/SeatModeration/Index.cshtml");
        }
    }
}