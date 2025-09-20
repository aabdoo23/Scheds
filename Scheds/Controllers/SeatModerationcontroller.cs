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


    }

}