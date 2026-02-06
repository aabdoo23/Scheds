using Scheds.Application.Interfaces.Repositories;
using Scheds.Application.Interfaces.Services;
using Scheds.Domain.Entities;
using Scheds.Infrastructure.Contexts;
using Scheds.Infrastructure.Util;

namespace Scheds.Infrastructure.Services
{
    public class CardItemService(
        ICardItemRepository cardItemRepository,
        ICourseScheduleRepository courseScheduleRepository,
        ICourseBaseRepository courseBaseRepository,
        SchedsDbContext context) : ICardItemService
    {
        private readonly ICardItemRepository _cardItemRepository = cardItemRepository
            ?? throw new ArgumentNullException(nameof(cardItemRepository));
        private readonly ICourseScheduleRepository _courseScheduleRepository = courseScheduleRepository
            ?? throw new ArgumentNullException(nameof(courseScheduleRepository));
        private readonly ICourseBaseRepository _courseBaseRepository = courseBaseRepository
            ?? throw new ArgumentNullException(nameof(courseBaseRepository));
        private readonly SchedsDbContext _context = context
            ?? throw new ArgumentNullException(nameof(context));
        public async Task LiveFetchUpsertAsync(CardItem cardItem)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                await UpsertCardAndSchedulesAsync(cardItem);
                await _courseBaseRepository.UpsertAsync(new CourseBase
                {
                    Id = cardItem.CourseCode,
                    CourseCode = cardItem.CourseCode,
                    CourseName = cardItem.CourseName,
                    LastUpdate = DateTime.UtcNow,
                });
                transaction.Commit();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task LiveFetchRefreshCourseAsync(string courseCode, List<CardItem> cards)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var existing = await _cardItemRepository.GetCardItemsByCourseCodeAsync(courseCode);
                var keepIds = cards.Select(c => c.Id).ToHashSet();
                foreach (var card in existing.Where(c => !keepIds.Contains(c.Id)))
                {
                    var schedules = await _courseScheduleRepository.GetCourseSchedulesByCardIdAsync(card.Id);
                    foreach (var s in schedules)
                        await _courseScheduleRepository.DeleteAsync(s.Id);
                    await _cardItemRepository.DeleteAsync(card.Id);
                }
                foreach (var card in cards)
                    await UpsertCardAndSchedulesAsync(card);
                var first = cards.FirstOrDefault();
                if (first != null)
                {
                    await _courseBaseRepository.UpsertAsync(new CourseBase
                    {
                        Id = first.CourseCode,
                        CourseCode = first.CourseCode,
                        CourseName = first.CourseName,
                        LastUpdate = DateTime.UtcNow,
                    });
                }
                transaction.Commit();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task UpsertCardAndSchedulesAsync(CardItem cardItem)
        {
            var updatedCard = await _cardItemRepository.UpsertAsync(cardItem);
            var existingSchedules = await _courseScheduleRepository.GetCourseSchedulesByCardIdAsync(updatedCard.Id);
            var newScheduleIds = cardItem.CourseSchedules.Select(s => s.Id).ToHashSet();
            foreach (var schedule in existingSchedules.Where(s => !newScheduleIds.Contains(s.Id)))
                await _courseScheduleRepository.DeleteAsync(schedule.Id);
            foreach (var schedule in cardItem.CourseSchedules)
            {
                schedule.CardItemId = updatedCard.Id;
                IdGenerationUtil.GenerateCourseScheduleId(schedule);
                await _courseScheduleRepository.UpsertAsync(schedule);
            }
        }
    }
}
