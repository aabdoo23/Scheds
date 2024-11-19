using Microsoft.AspNetCore.Mvc;

namespace Scheds.MVC.Controllers
{
    public class FindStudyRoomsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}