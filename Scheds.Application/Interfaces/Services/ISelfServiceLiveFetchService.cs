using Scheds.Domain.Entities;

namespace Scheds.Application.Interfaces.Services
{
    public interface ISelfServiceLiveFetchService
    {
        public Task<List<CardItem>> FetchCards(string CourseCode);
        public Task FetchCourseBases(string CourseCode);
    }
}
