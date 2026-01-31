using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Scheds.Domain.Configuration;

namespace Scheds.MVC.Controllers
{
    public class GenerateSchedulesController : Controller
    {
        private readonly FrontendSettings _frontend;

        public GenerateSchedulesController(IOptions<FrontendSettings> frontend)
        {
            _frontend = frontend.Value;
        }

        public IActionResult Index()
        {
            return Redirect($"{_frontend.Url.TrimEnd('/')}/generate-schedules");
        }
    }
}
