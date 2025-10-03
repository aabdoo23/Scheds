using Microsoft.EntityFrameworkCore;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Domain.Entities;
using Scheds.Infrastructure.Contexts;
using Scheds.Infrastructure.Repositories.Common;

namespace Scheds.Infrastructure.Repositories
{
    public class CartSeatModerationRepository(SchedsDbContext context) 
        : BaseRepository<CartSeatModeration>(context), ICartSeatModerationRepository
    {
        private readonly SchedsDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<CartSeatModeration?> GetByUserAndCourseAsync(string userId, string courseCode, string section)
        {
            return await _context.CartSeatModerations
                .FirstOrDefaultAsync(c => c.UserId == userId && c.CourseCode == courseCode && c.Section == section);
        }

        public async Task<List<CartSeatModeration>> GetUserCartItemsAsync(string userId)
        {
            return await _context.CartSeatModerations
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task ClearUserCartAsync(string userId)
        {
            var cartItems = await _context.CartSeatModerations
                .Where(c => c.UserId == userId)
                .ToListAsync();

            _context.CartSeatModerations.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }
    }
}
