using Scheds.Domain.Entities;
using System.Threading;

namespace Scheds.Application.Interfaces.Services
{
    public interface ISeatModerationService
    {
        Task<List<CardItem>> FetchAndProcessCourseData(List<string> courseCodes, List<string> sections);
        Task MoniterAllCourses(CancellationToken cancellationToken);
        Task SubscribeUserToMonitoring(string userEmail, List<string> courseSections);
        Task UnsubscribeUserFromMonitoring(string userEmail, List<string> courseSections);
        Task AddToSeatModerationCart(string userEmail, string courseCode, string section);
        Task RemoveFromSeatModerationCart(string userEmail, string courseCode, string section);
        Task<List<CartSeatModeration>> GetSeatModerationCart(string userEmail);
        Task ClearSeatModerationCart(string userEmail);
    }
}