using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Scheds.Domain.Configuration;

namespace Scheds.MVC.Controllers
{
    public class FindStudyRoomsController : Controller
    {
        private readonly FrontendSettings _frontend;

        public FindStudyRoomsController(IOptions<FrontendSettings> frontend)
        {
            _frontend = frontend.Value;
        }

        public IActionResult Index()
        {
            return Redirect($"{_frontend.Url.TrimEnd('/')}/find-study-rooms");
        }
    }
}
