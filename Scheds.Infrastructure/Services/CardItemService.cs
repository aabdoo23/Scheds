using Scheds.Application.Interfaces.Repositories;
using Scheds.Application.Interfaces.Services;
using Scheds.Domain.Entities;

namespace Scheds.Infrastructure.Services
{
    public class CardItemService(
        ICardItemRepository cardItemRepository,
        ICourseScheduleRepository courseScheduleRepository,
        ICourseBaseRepository courseBaseRepository) : ICardItemService
    {
        private readonly ICardItemRepository _cardItemRepository = cardItemRepository
            ?? throw new ArgumentNullException(nameof(cardItemRepository));
        private readonly ICourseScheduleRepository _courseScheduleRepository = courseScheduleRepository
            ?? throw new ArgumentNullException(nameof(courseScheduleRepository));
        private readonly ICourseBaseRepository _courseBaseRepository = courseBaseRepository
            ?? throw new ArgumentNullException(nameof(courseBaseRepository));
        public async Task LiveFetchUpsertAsync(CardItem cardItem)
        {
            var updatedCard = await _cardItemRepository.UpsertAsync(cardItem);

            foreach (var schedule in cardItem.CourseSchedules)
            {
                schedule.CardItemId = updatedCard.Id;
                await _courseScheduleRepository.UpsertAsync(schedule);
            }

            CourseBase courseBase = new()
            {
                Id = cardItem.CourseCode,
                CourseCode = cardItem.CourseCode,
                CourseName = cardItem.CourseName
            };
            await _courseBaseRepository.UpsertAsync(courseBase);

        }
    }
}
