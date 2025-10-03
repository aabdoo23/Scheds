using Microsoft.EntityFrameworkCore;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Domain.Entities;
using Scheds.Infrastructure.Contexts;
using Scheds.Infrastructure.Repositories.Common;

namespace Scheds.Infrastructure.Repositories
{
    public class SeatModerationRepository(SchedsDbContext context) 
        : BaseRepository<SeatModeration>(context), ISeatModerationRepository
    {
        private readonly SchedsDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<SeatModeration?> GetByIdWithUsersAsync(string id)
        {
            return await _context.SeatModerations
                .Include(sm => sm.Users)
                .FirstOrDefaultAsync(sm => sm.Id == id);
        }

        public async Task<List<SeatModeration>> GetAllWithUsersAsync()
        {
            return await _context.SeatModerations
                .Include(sm => sm.Users)
                .ToListAsync();
        }

        public async Task<List<SeatModeration>> GetUserSeatModerationsAsync(string userEmail)
        {
            return await _context.SeatModerations
                .Include(sm => sm.Users)
                .Where(sm => sm.Users.Any(u => u.Email == userEmail))
                .ToListAsync();
        }
    }
}
