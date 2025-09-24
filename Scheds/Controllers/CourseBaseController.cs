using Microsoft.AspNetCore.Mvc;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Application.Interfaces.Services;
using Scheds.Domain.Entities;
using Scheds.Domain.DTOs;
using System.Text.Json;

namespace Scheds.MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseBaseController(ICourseBaseRepository courseBaseRepository, ICardItemRepository cardItemRepository, ISelfServiceLiveFetchService selfServiceLiveFetchService ,ICourseBaseService courseBaseService) : Controller
    {

        private readonly ICourseBaseService _courseBaseService = courseBaseService
            ?? throw new ArgumentNullException(nameof(courseBaseService));
        private readonly ICourseBaseRepository _courseBaseRepository = courseBaseRepository
            ?? throw new ArgumentNullException(nameof(courseBaseRepository));
        private readonly ICardItemRepository _cardItemRepository = cardItemRepository
            ?? throw new ArgumentNullException(nameof(cardItemRepository));
        private readonly ISelfServiceLiveFetchService _selfServiceLiveFetchService = selfServiceLiveFetchService
            ?? throw new ArgumentNullException(nameof(selfServiceLiveFetchService));

        [HttpGet("get-filtered-courses")]
        public async Task<IActionResult> GetAllCourses([FromQuery] string query = "")
        {
            var courses = await _courseBaseService.GetFilteredCourses(query);
            return Ok(courses);
        }

        [HttpPost("get-course-sections")]
        public async Task<IActionResult> GetCourseSections([FromBody] CourseSectionsRequestDTO request)
        {
            var sections = await _courseBaseService.GetCourseSections(request.CourseCode);
            return Ok(sections);
        }

        [HttpGet("search/{query}")]
        public async Task<IActionResult> SearchCourses(string query)
        {
            await _selfServiceLiveFetchService.FetchCourseBases(query);
            return Ok();
        }

        [HttpGet("getCourseBaseByCourseCode")]
        public async Task<IActionResult> GetCourseBaseByCourseCode(string courseCode)
        {
            var course = await _courseBaseRepository.GetCourseBaseByCourseCodeAsync(courseCode);
            return Ok(course);
        }

        [HttpGet("getCourseBaseByName")]
        public async Task<IActionResult> GetCourseBaseByName(string courseName)
        {
            var course = await _courseBaseRepository.GetCourseBaseByCourseNameAsync(courseName);
            return Ok(course);
        }

        [HttpPost("addCourseBase")]
        public async Task<IActionResult> AddCourseBase(CourseBase courseBase)
        {
            await _courseBaseRepository.InsertAsync(courseBase);
            return Ok();
        }

        [HttpGet("SelfSearch")]
        public async Task<IActionResult> SelfSearch(string query)
        {
            var cardItems = await _cardItemRepository.Search(query);
            var dtoItems = cardItems.Select(item => new ReturnedCardItemDTO(item)).ToList();
            
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                WriteIndented = true
            };
            
            return new JsonResult(dtoItems, options);
        }
    }
}
