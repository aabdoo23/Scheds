using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Scheds.Domain.Configuration;

namespace Scheds.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly FrontendSettings _frontend;

        public AccountController(IOptions<FrontendSettings> frontend)
        {
            _frontend = frontend.Value;
        }

        [HttpGet("api/account/me")]
        public IActionResult Me()
        {
            if (User?.Identity?.IsAuthenticated != true)
                return Unauthorized();
            return Ok(new { name = User.Identity!.Name });
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = "/")
        {
            var frontendBase = _frontend.Url?.TrimEnd('/') ?? "";
            var allowed = string.IsNullOrEmpty(returnUrl)
                || returnUrl.StartsWith('/')
                || (frontendBase.Length > 0 && returnUrl.StartsWith(frontendBase, StringComparison.OrdinalIgnoreCase));
            if (!allowed)
                returnUrl = frontendBase + "/";

            var properties = new AuthenticationProperties { RedirectUri = returnUrl };
            return Challenge(properties, "Google");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
