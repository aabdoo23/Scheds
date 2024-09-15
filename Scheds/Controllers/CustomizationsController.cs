using Microsoft.AspNetCore.Mvc;
using Scheds.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Scheds.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CustomizationsController : ControllerBase
    {
        private const string CookieKeyCart = "customizations";
        //TODO: Implement customizations controller with cookie storage
    }
}