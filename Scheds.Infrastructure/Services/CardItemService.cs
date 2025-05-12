using Microsoft.EntityFrameworkCore;
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
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var updatedCard = await _cardItemRepository.UpsertAsync(cardItem);

                    var existingSchedules = await _courseScheduleRepository
                        .GetCourseSchedulesByCardIdAsync(updatedCard.Id);

                    var newScheduleIds = cardItem.CourseSchedules.Select(s => s.Id).ToHashSet();
                    var schedulesToDelete = existingSchedules.Where(s => !newScheduleIds.Contains(s.Id));

                    foreach (var schedule in schedulesToDelete)
                    {
                        await _courseScheduleRepository.DeleteAsync(schedule.Id);
                    }

                    // Upsert new schedules
                    foreach (var schedule in cardItem.CourseSchedules)
                    {
                        schedule.CardItemId = updatedCard.Id;
                        IdGenerationUtil.GenerateCourseScheduleId(schedule); // Ensure ID is set
                        await _courseScheduleRepository.UpsertAsync(schedule);
                    }

                    // Upsert CourseBase
                    var courseBase = new CourseBase
                    {
                        Id = cardItem.CourseCode,
                        CourseCode = cardItem.CourseCode,
                        CourseName = cardItem.CourseName,
                        LastUpdate = DateTime.Now,
                    };
                    await _courseBaseRepository.UpsertAsync(courseBase);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    
    }
}
