using Microsoft.EntityFrameworkCore;
using Scheds.Models;

namespace Scheds.DAL.Repositories
{
    public class CardItemRepository
    {
        private readonly SchedsDbContext _context;

        public CardItemRepository(SchedsDbContext context)
        {
            _context = context;
        }

        public async Task<List<CardItem>> GetAllCardItemsAsync()
        {
            return await _context.Sections_Fall25.Include(c => c.Schedule).ToListAsync();
        }

        public async Task<List<CardItem>> GetCardItemsByCourseCodeAsync(string courseCode)
        {
            return await _context.Sections_Fall25.Include(c => c.Schedule)
                .Where(cardItem => cardItem.CourseCode == courseCode)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<CardItem> GetCardItemByIdAsync(int id)
        {
            return await _context.Sections_Fall25.Include(c => c.Schedule)
                .Where(cardItem => cardItem.CardId == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task UpdateCardItemAsync(CardItem cardItem)
        {
            var existingEntity = _context.Sections_Fall25.Local
                .FirstOrDefault(c => c.CardId == cardItem.CardId);

            if (existingEntity != null)
            {
                // Detach the existing tracked entity
                _context.Entry(existingEntity).State = EntityState.Detached;
            }

            // Now attach and update the new entity
            _context.Sections_Fall25.Update(cardItem);
            await _context.SaveChangesAsync();
        }

    }
}
