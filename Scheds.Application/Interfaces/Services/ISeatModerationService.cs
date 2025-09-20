using Scheds.Domain.Entities;

namespace Scheds.Application.Interfaces.Services
{
    public interface ISeatModerationService
    {
        Task<List<CardItem>> FetchAndProcessCourseData(List<string> courseCodes, List<string> sections);
    }
}