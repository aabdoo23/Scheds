using Scheds.Domain.Entities;
using Scheds.Domain.ValueObjects;

namespace Scheds.Application.Interfaces.Services
{
    public interface ISelfServiceLiveFetchService
    {
        public Task<List<CardItem>> FetchCards(string CourseCode);
        public Task<bool> FetchCourseBases(string CourseCode);

        /// <summary>
        /// Returns the current card for each requested course-section pair. Pairs are passed
        /// as pairs rather than two parallel lists so callers cannot misalign them.
        /// </summary>
        public Task<List<CardItem>> FetchSeatData(IReadOnlyCollection<CourseSection> requested);
    }
}
