using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Scheds.Domain.Configuration;
using Scheds.Domain.ViewModels;

namespace Scheds.MVC.Controllers;

public class HomeController : Controller
{
    private readonly FrontendSettings _frontend;

    public HomeController(IOptions<FrontendSettings> frontend)
    {
        _frontend = frontend.Value;
    }

    public IActionResult Index()
    {
        return Redirect(_frontend.Url);
    }

    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
    }
}
