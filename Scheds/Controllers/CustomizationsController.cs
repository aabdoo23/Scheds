using Microsoft.AspNetCore.Mvc;

namespace Scheds.MVC.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CustomizationsController : ControllerBase
    {
        private const string CookieKeyCart = "customizations";
        //TODO: Implement customizations controller with cookie storage
    }
}