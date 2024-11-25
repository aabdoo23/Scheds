using Microsoft.AspNetCore.Mvc;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Domain.Entities;

namespace Scheds.MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InstructorController(IInstructorRepository repository) : Controller
    {
        private readonly IInstructorRepository _repository = repository 
            ?? throw new ArgumentNullException(nameof(repository));

        [HttpGet("getAllInstructors")]
        public async Task<ActionResult<List<Instructor>>> GetAllInstructors()
        {
            var instructors = await _repository.GetAllAsync();
            return instructors.ToList();
        }
    }
}
