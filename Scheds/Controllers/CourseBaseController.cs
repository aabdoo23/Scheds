using Microsoft.AspNetCore.Mvc;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Application.Interfaces.Services;
using Scheds.Domain.Entities;

namespace Scheds.MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseBaseController(ICourseBaseRepository courseBaseRepository, ISelfServiceLiveFetchService selfServiceLiveFetchService) : Controller
    {
        private readonly ICourseBaseRepository _courseBaseRepository = courseBaseRepository
            ?? throw new ArgumentNullException(nameof(courseBaseRepository));
        private readonly ISelfServiceLiveFetchService _selfServiceLiveFetchService = selfServiceLiveFetchService
            ?? throw new ArgumentNullException(nameof(selfServiceLiveFetchService));

        [HttpGet("getAllCourses")]
        public async Task<IActionResult> GetAllCourses()
        {
            var courses = await courseBaseRepository.GetAllAsync();
            return Ok(courses);
        }

        [HttpGet("search/{query}")]
        public async Task<IActionResult> SearchCourses(string query)
        {
            var courses = await _selfServiceLiveFetchService.FetchCourseBases(query);
            return Ok(courses);
        }

        [HttpGet("getCourseBaseByCourseCode")]
        public async Task<IActionResult> GetCourseBaseByCourseCode(string courseCode)
        {
            var course = await courseBaseRepository.GetCourseBaseByCourseCodeAsync(courseCode);
            return Ok(course);
        }

        [HttpGet("getCourseBaseByName")]
        public async Task<IActionResult> GetCourseBaseByName(string courseName)
        {
            var course = await courseBaseRepository.GetCourseBaseByCourseNameAsync(courseName);
            return Ok(course);
        }

        [HttpPost("addCourseBase")]
        public async Task<IActionResult> AddCourseBase(CourseBase courseBase)
        {
            await courseBaseRepository.InsertAsync(courseBase);
            return Ok();
        }
    }
}
