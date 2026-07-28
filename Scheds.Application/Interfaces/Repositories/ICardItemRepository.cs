using Scheds.Application.Interfaces.Repositories.Common;
using Scheds.Domain.Entities;

namespace Scheds.Application.Interfaces.Repositories
{
    public interface ICardItemRepository : IBaseRepository<CardItem>
    {
        public Task<List<CardItem>> GetCardItemsByCourseCodeAsync(string courseCode);

        /// <summary>
        /// Cards for a course without their schedules, for callers that only need seat counts.
        /// </summary>
        /// <remarks>
        /// <see cref="CardItem.CourseSchedules"/> is left empty, so the schedule-dependent members
        /// on <see cref="CardItem"/> must not be called on the results.
        /// </remarks>
        public Task<List<CardItem>> GetSeatDataByCourseCodeAsync(string courseCode);

        public Task<List<CardItem>> Search(string query);
    }
}
