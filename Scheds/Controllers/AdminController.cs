using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Scheds.Domain.Configuration;

namespace Scheds.MVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly FrontendSettings _frontend;

        public AdminController(IOptions<FrontendSettings> frontend)
        {
            _frontend = frontend.Value;
        }

        public IActionResult Login()
        {
            return Redirect($"{_frontend.Url.TrimEnd('/')}/admin/login");
        }

        [HttpPost]
        public IActionResult Login(string password)
        {
            return Redirect($"{_frontend.Url.TrimEnd('/')}/admin/login");
        }

        public IActionResult Logout()
        {
            return Redirect($"{_frontend.Url.TrimEnd('/')}/admin/login");
        }

        public IActionResult Index()
        {
            return Redirect($"{_frontend.Url.TrimEnd('/')}/admin");
        }

        public IActionResult Analytics()
        {
            return Redirect($"{_frontend.Url.TrimEnd('/')}/admin/analytics");
        }

        public IActionResult GenerationHistory()
        {
            return Redirect($"{_frontend.Url.TrimEnd('/')}/admin/generations");
        }

        public IActionResult GenerationDetails(int id)
        {
            return Redirect($"{_frontend.Url.TrimEnd('/')}/admin/generations/{id}");
        }
    }
}
