using Microsoft.AspNetCore.Mvc;
using Scheds.Application.Interfaces.Services;
using Scheds.Domain.Entities;
using Scheds.Domain.DTOs;

namespace Scheds.MVC.Controllers
{
    public class SeatModerationController : Controller
    {
        private readonly ISeatModerationService _seatModerationService;

        public SeatModerationController(ISeatModerationService seatModerationService)
        {
            _seatModerationService = seatModerationService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("api/SeatModeration/check-seats")]
        public async Task<IActionResult> CheckSeats([FromBody] CourseDataRequestSeatModeration request)
        {
            try
            {
                var results = await _seatModerationService.FetchAndProcessCourseData(request.CourseCode, request.Sections);

                var response = new 
                { 
                    Success = true,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    RequestedCourses = request.CourseCode,
                    RequestedSections = request.Sections,
                    Results = results.Select(c => new 
                    {
                        Course = c.CourseCode,
                        CourseName = c.CourseName,
                        Section = c.Section,
                        HasSeats = c.SeatsLeft > 0,
                        SeatsLeft = c.SeatsLeft,
                        LastUpdate = c.LastUpdate.ToString("yyyy-MM-dd HH:mm:ss"),
                        Instructor = c.Instructor
                    })
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/SeatModeration/subscribe")]
        public async Task<IActionResult> SubscribeToMonitoring([FromBody] SubscribeRequest request)
        {
            try
            {
                if (User?.Identity?.IsAuthenticated != true)
                {
                    return Unauthorized(new { Success = false, Error = "Authentication required" });
                }

                var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest(new { Success = false, Error = "User email not found" });
                }

                await _seatModerationService.SubscribeUserToMonitoring(userEmail, request.CourseSections);

                return Ok(new { Success = true, Message = "Successfully subscribed to seat monitoring" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/SeatModeration/cart/add")]
        public async Task<IActionResult> AddToSeatModerationCart([FromBody] AddToSeatModerationCartRequest request)
        {
            try
            {
                if (User?.Identity?.IsAuthenticated != true)
                {
                    return Unauthorized(new { Success = false, Error = "Authentication required" });
                }

                var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest(new { Success = false, Error = "User email not found" });
                }

                await _seatModerationService.AddToSeatModerationCart(userEmail, request.CourseCode, request.Section);

                return Ok(new { Success = true, Message = "Course added to seat moderation cart" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/SeatModeration/cart/remove")]
        public async Task<IActionResult> RemoveFromSeatModerationCart([FromBody] RemoveFromSeatModerationCartRequest request)
        {
            try
            {
                if (User?.Identity?.IsAuthenticated != true)
                {
                    return Unauthorized(new { Success = false, Error = "Authentication required" });
                }

                var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest(new { Success = false, Error = "User email not found" });
                }

                await _seatModerationService.RemoveFromSeatModerationCart(userEmail, request.CourseCode, request.Section);

                return Ok(new { Success = true, Message = "Course removed from seat moderation cart" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/SeatModeration/cart/get")]
        public async Task<IActionResult> GetSeatModerationCart()
        {
            try
            {
                if (User?.Identity?.IsAuthenticated != true)
                {
                    return Unauthorized(new { Success = false, Error = "Authentication required" });
                }

                var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest(new { Success = false, Error = "User email not found" });
                }

                var cartItems = await _seatModerationService.GetSeatModerationCart(userEmail);

                return Ok(new { Success = true, CartItems = cartItems });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/SeatModeration/cart/clear")]
        public async Task<IActionResult> ClearSeatModerationCart()
        {
            try
            {
                if (User?.Identity?.IsAuthenticated != true)
                {
                    return Unauthorized(new { Success = false, Error = "Authentication required" });
                }

                var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest(new { Success = false, Error = "User email not found" });
                }

                await _seatModerationService.ClearSeatModerationCart(userEmail);

                return Ok(new { Success = true, Message = "Seat moderation cart cleared" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/SeatModeration/unsubscribe")]
        public async Task<IActionResult> UnsubscribeFromMonitoring([FromBody] UnsubscribeRequest request)
        {
            try
            {
                if (User?.Identity?.IsAuthenticated != true)
                {
                    return Unauthorized(new { Success = false, Error = "Authentication required" });
                }

                var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest(new { Success = false, Error = "User email not found" });
                }

                await _seatModerationService.UnsubscribeUserFromMonitoring(userEmail, request.CourseSections);

                return Ok(new { Success = true, Message = "Successfully unsubscribed from seat monitoring" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Error = ex.Message });
            }
        }


    }

}