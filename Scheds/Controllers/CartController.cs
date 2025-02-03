using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Scheds.Domain.DTOs;
using Scheds.Domain.Entities;

namespace Scheds.MVC.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private const string CookieKeyCart = "CartItems";
        private const string GenerateRequestCookieKey = "GenerateRequest";

        [HttpPost("add")]
        public IActionResult AddToCart([FromBody] CourseBase course)
        {
            var cart = GetCartItemsFromCookies();
            if (!cart.Exists(c => c.CourseCode == course.CourseCode))
            {
                cart.Add(course);
                SaveCartItemsToCookies(cart);
            }
            return Ok();
        }
        // Remove from cart (POST)
        [HttpPost("remove")]
        public IActionResult RemoveFromCart([FromBody] CourseBase course)
        {
            var cart = GetCartItemsFromCookies();
            var itemToRemove = cart.Find(c => c.CourseCode == course.CourseCode);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                SaveCartItemsToCookies(cart);
            }
            return Ok();
        }

        // Retrieve cart items (GET)
        [HttpGet("getCartItems")]
        public IActionResult GetCartItems()
        {
            var cart = GetCartItemsFromCookies();
            return Ok(cart);
        }

        // Helper method to get the cart from cookies
        private List<CourseBase> GetCartItemsFromCookies()
        {
            var cartJson = Request.Cookies[CookieKeyCart];
            if (string.IsNullOrEmpty(cartJson))
            {
                Console.WriteLine("Cart is empty");
                return new List<CourseBase>();
            }
            return JsonConvert.DeserializeObject<List<CourseBase>>(cartJson);
        }

        // Helper method to save the cart to cookies
        private void SaveCartItemsToCookies(List<CourseBase> cart)
        {
            var cartJson = JsonConvert.SerializeObject(cart);
            CookieOptions options = new()
            {
                Expires = DateTime.Now.AddMonths(1) // Set cookie to expire in one day
            };
            Response.Cookies.Append(CookieKeyCart, cartJson, options);
        }
        private void SaveGenerateRequestToCookies(GenerateRequestDTO generateRequest)
        {
            var generateRequestJson = JsonConvert.SerializeObject(generateRequest);
            CookieOptions options = new()
            {
                Expires = DateTime.Now.AddMonths(1) // Set cookie to expire in one day
            };
            //update cookie
            Response.Cookies.Append(GenerateRequestCookieKey, generateRequestJson, options);
        }
        private GenerateRequestDTO GetGenerateRequestFromCookies()
        {
            var generateRequestJson = Request.Cookies[GenerateRequestCookieKey];
            if (string.IsNullOrEmpty(generateRequestJson))
            {
                Console.WriteLine("GenerateRequest is empty");
                return new GenerateRequestDTO();
            }
            return JsonConvert.DeserializeObject<GenerateRequestDTO>(generateRequestJson);
        }
        [HttpPost("generate")]
        public IActionResult GenerateRequest([FromBody] GenerateRequestDTO generateRequest)
        {
            SaveGenerateRequestToCookies(generateRequest);
            return Ok();
        }
        [HttpGet("getGenerateRequest")]
        public IActionResult GetGenerateRequest()
        {
            var generateRequest = GetGenerateRequestFromCookies();
            return Ok(generateRequest);
        }
        [HttpPost("clear")]
        public IActionResult ClearCart()
        {
            // Delete the cart cookie
            Response.Cookies.Delete(CookieKeyCart);
            // Delete the generate request cookie as well since it depends on cart items
            Response.Cookies.Delete(GenerateRequestCookieKey);
            return Ok();
        }
    }
}
