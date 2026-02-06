using Scheds.Domain.Entities;

namespace Scheds.Application.Interfaces.Services
{
    public interface ISelfServiceLiveFetchService
    {
        public Task<List<CardItem>> FetchCards(string CourseCode);
        public Task<bool> FetchCourseBases(string CourseCode);
        public Task<List<CardItem>> FetchCardsSeatModeration(List<string> CourseCodes, List<string> sections);
    }
}
