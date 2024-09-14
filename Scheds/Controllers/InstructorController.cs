using Microsoft.AspNetCore.Mvc;
using Scheds.DAL.Repositories;
using Scheds.Model;

namespace Scheds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InstructorController : Controller
    {
        private readonly AllInstructorsRepository repository;
        public InstructorController(AllInstructorsRepository repository)
        {
            this.repository = repository;
        }
        [HttpGet("getAllInstructors")]
        public async Task<ActionResult<List<Instructor>>> GetAllInstructors(){
            var instructors = await repository.GetAllInstructorsAsync();
            return instructors;
        }

        
    }
}
