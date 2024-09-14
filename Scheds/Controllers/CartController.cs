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
        private const string SessionKeyCart = "CartItems";

        // Add to cart (POST)
        [HttpPost("add")]
        public IActionResult AddToCart([FromBody] CourseBase course)
        {
            var cart = GetCartItemsFromSession();
            if (!cart.Exists(c => c.CourseCode == course.CourseCode))
            {
                cart.Add(course);
                SaveCartItemsToSession(cart);
            }
            return Ok();
        }

        // Remove from cart (POST)
        [HttpPost("remove")]
        public IActionResult RemoveFromCart([FromBody] CourseBase course)
        {
            var cart = GetCartItemsFromSession();
            var itemToRemove = cart.Find(c => c.CourseCode == course.CourseCode);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                SaveCartItemsToSession(cart);
            }
            return Ok();
        }

        // Retrieve cart items (GET)
        [HttpGet("getCartItems")]
        public IActionResult GetCartItems()
        {
            var cart = GetCartItemsFromSession();
            return Ok(cart);
        }

        // Helper method to get the cart from the session
        private List<CourseBase> GetCartItemsFromSession()
        {
            var cartJson = HttpContext.Session.GetString(SessionKeyCart);
            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CourseBase>();
            }
            return JsonConvert.DeserializeObject<List<CourseBase>>(cartJson);
        }

        // Helper method to save the cart to the session
        private void SaveCartItemsToSession(List<CourseBase> cart)
        {
            var cartJson = JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString(SessionKeyCart, cartJson);
        }
    }
}
