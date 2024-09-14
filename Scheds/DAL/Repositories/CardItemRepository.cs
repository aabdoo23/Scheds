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
            return await _context.Sections_Spr24.Include(c => c.Schedule).ToListAsync();
        }

        public async Task<List<CardItem>> GetCardItemsByCourseCodeAsync(string courseCode)
        {
            return await _context.Sections_Spr24.Include(c => c.Schedule)
                .Where(cardItem => cardItem.CourseCode == courseCode)
                .ToListAsync();
        }

        public async Task<CardItem> GetCardItemByIdAsync(int id)
        {
            return await _context.Sections_Spr24.Include(c => c.Schedule)
                .Where(cardItem => cardItem.CardId == id)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateCardItemAsync(CardItem cardItem)
        {
            _context.Sections_Spr24.Update(cardItem);
            await _context.SaveChangesAsync();
        }
    }
}
