using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Scheds.Application.Interfaces.Services;
using Scheds.Domain.Configuration;
using Scheds.Domain.Entities;
using Scheds.Domain.DTOs;

namespace Scheds.MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeatModerationController : ControllerBase
    {
        private readonly ISeatModerationService _seatModerationService;
        private readonly FrontendSettings _frontend;

        public SeatModerationController(ISeatModerationService seatModerationService, IOptions<FrontendSettings> frontend)
        {
            _seatModerationService = seatModerationService ?? throw new ArgumentNullException(nameof(seatModerationService));
            _frontend = frontend.Value;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok("SeatModeration API is running");
        }

        [HttpGet("~/SeatModeration")]
        public IActionResult ViewIndex()
        {
            return Redirect($"{_frontend.Url.TrimEnd('/')}/seat-moderation");
        }

        [HttpPost("check-seats")]
        public async Task<IActionResult> CheckSeats([FromBody] CourseDataRequestSeatModerationDTO request)
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

        [HttpPost("cart/add")]
        public async Task<IActionResult> AddToSeatModerationCart([FromBody] AddToSeatModerationCartRequestDTO request)
        {
            try
            {
                var userEmail = GetUserEmail();
                if (userEmail == null)
                    return Unauthorized(new { Success = false, Error = "Authentication required or email not found" });

                await _seatModerationService.AddToSeatModerationCart(userEmail, request.CourseCode, request.Section);
                return Ok(new { Success = true, Message = "Course added to seat moderation cart" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Error = ex.Message });
            }
        }

        [HttpPost("cart/remove")]
        public async Task<IActionResult> RemoveFromSeatModerationCart([FromBody] CartItemDTO request)
        {
            try
            {
                var userEmail = GetUserEmail();
                if (userEmail == null)
                    return Unauthorized(new { Success = false, Error = "Authentication required or email not found" });

                await _seatModerationService.RemoveFromSeatModerationCart(userEmail, request.CourseCode, request.Section);
                return Ok(new { Success = true, Message = "Course removed from seat moderation cart" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Error = ex.Message });
            }
        }

        [HttpGet("cart")]
        public async Task<IActionResult> GetSeatModerationCart()
        {
            try
            {
                var userEmail = GetUserEmail();
                if (userEmail == null)
                    return Unauthorized(new { Success = false, Error = "Authentication required or email not found" });

                var cartItems = await _seatModerationService.GetSeatModerationCart(userEmail);
                return Ok(new { Success = true, CartItems = cartItems });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Error = ex.Message });
            }
        }

        [HttpPost("cart/clear")]
        public async Task<IActionResult> ClearSeatModerationCart()
        {
            try
            {
                var userEmail = GetUserEmail();
                if (userEmail == null)
                    return Unauthorized(new { Success = false, Error = "Authentication required or email not found" });

                await _seatModerationService.ClearSeatModerationCart(userEmail);
                return Ok(new { Success = true, Message = "Seat moderation cart cleared" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Error = ex.Message });
            }
        }

        [HttpGet("active-jobs")]
        public async Task<IActionResult> GetActiveMonitoringJobs()
        {
            try
            {
                var userEmail = GetUserEmail();
                if (userEmail == null)
                    return Unauthorized(new { Success = false, Error = "Authentication required or email not found" });

                var activeJobs = await _seatModerationService.GetUserActiveMonitoringJobs(userEmail);
                
                var response = new 
                { 
                    Success = true,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    ActiveJobs = activeJobs.Select(c => new 
                    {
                        Course = c.CourseCode,
                        CourseName = c.CourseName,
                        Section = c.Section,
                        HasSeats = c.SeatsLeft > 0,
                        SeatsLeft = c.SeatsLeft,
                        LastUpdate = c.LastUpdate.ToString("yyyy-MM-dd HH:mm:ss"),
                        Instructor = c.Instructor
                    }).ToList()
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Error = ex.Message });
            }
        }

        private string? GetUserEmail()
        {
            if (User?.Identity?.IsAuthenticated != true)
                return null;
            
            return User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        }
    }
}