using Microsoft.AspNetCore.Mvc;
using Scheds.Application.Interfaces.Services;
using Scheds.Domain.Entities;
using Scheds.Domain.DTOs;

namespace Scheds.MVC.Controllers
{
    [Route("api/[controller]")]
    public class SeatModerationController : ControllerBase
    {
        private readonly ISeatModerationService _seatModerationService;

        public SeatModerationController(ISeatModerationService seatModerationService)
        {
            _seatModerationService = seatModerationService;
        }

        [HttpPost("check-seats")]
        public async Task<IActionResult> CheckSeats([FromBody] CourseDataRequestSeatModeration request)
        {
            try
            {
                var results = await _seatModerationService.FetchAndProcessCourseData(request.CourseCode, request.Sections);

                return Ok(new 
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
                        LastUpdate = c.LastUpdate.ToString("yyyy-MM-dd HH:mm:ss")
                    })
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Error = ex.Message });
            }
        }


    }

}