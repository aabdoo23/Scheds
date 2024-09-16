using Microsoft.EntityFrameworkCore;
using Scheds.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scheds.DAL.Repositories
{
    public class AllInstructorsRepository
    {
        private readonly SchedsDbContext _context;

        public AllInstructorsRepository(SchedsDbContext context)
        {
            _context = context;
        }

        public async Task<List<Instructor>> GetAllInstructorsAsync()
        {
            return await _context.All_Instructors_Fall25.ToListAsync();
        }

        public async Task<string> GetInstructorNameById(int id)
        {
            var instructor = await _context.All_Instructors_Fall25
                .Where(ins => ins.Id == id)
                .FirstOrDefaultAsync();

            return instructor?.FullName; 
        }
    }
}
