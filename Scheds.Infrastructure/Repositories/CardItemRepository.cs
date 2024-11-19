using Microsoft.EntityFrameworkCore;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Domain.Entities;
using Scheds.Infrastructure.Contexts;
using Scheds.Infrastructure.Repositories.Common;

namespace Scheds.Infrastructure.Repositories
{
    public class CardItemRepository(SchedsDbContext context) : BaseRepository<CardItem>(context), ICardItemRepository
    {
        private readonly SchedsDbContext _context = context
            ?? throw new ArgumentNullException(nameof(context));

        public async Task<List<CardItem>> GetCardItemsByCourseCodeAsync(string courseCode)
        {
            return await _context.CardItems.Include(c => c.CourseSchedules)
                .Where(cardItem => cardItem.CourseCode == courseCode)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
