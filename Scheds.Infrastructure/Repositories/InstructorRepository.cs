using Microsoft.EntityFrameworkCore;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Domain.Entities;
using Scheds.Infrastructure.Contexts;
using Scheds.Infrastructure.Repositories.Common;

namespace Scheds.Infrastructure.Repositories
{
    public class InstructorRepository(SchedsDbContext context) : BaseRepository<Instructor>(context), IInstructorRepository
    {
        private readonly SchedsDbContext _context = context 
            ?? throw new ArgumentNullException(nameof(context));

        public async Task<string?> GetInstructorNameById(string id)
        {
            var instructor = await _context.Instructors
                .Where(ins => ins.Id == id)
                .FirstOrDefaultAsync();

            return instructor?.FullName;
        }
    }
}
