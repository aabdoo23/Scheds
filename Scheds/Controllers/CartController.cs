using Microsoft.AspNetCore.Mvc;
using Scheds.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Scheds.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private const string CookieKeyCart = "CartItems";

        // Add to cart (POST)
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
                System.Console.WriteLine("Cart is empty");
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
                Expires = DateTime.Now.AddDays(1) // Set cookie to expire in one day
            };
            Response.Cookies.Append(CookieKeyCart, cartJson, options);
        }



    }
}
