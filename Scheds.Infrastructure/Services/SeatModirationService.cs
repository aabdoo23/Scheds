using Scheds.Application.Interfaces.Repositories;
using Scheds.Application.Interfaces.Services;
using Scheds.Domain.Entities;

namespace Scheds.Infrastructure.Services
{
    public class SeatModerationService(ISelfServiceLiveFetchService selfServiceLiveFetchService) : ISeatModerationService
    {
        private readonly ISelfServiceLiveFetchService _selfServiceLiveFetchService = selfServiceLiveFetchService
            ?? throw new ArgumentNullException(nameof(selfServiceLiveFetchService));

        public async Task<List<CardItem>> FetchAndProcessCourseData(List<string> courseCodes, List<string> sections)
        {
            try
            {
                var fetchedCards = await _selfServiceLiveFetchService.FetchCardsSeatModeration(courseCodes, sections);
                
                Console.WriteLine($"[API] Fetched {fetchedCards.Count} courses for frontend");
                foreach (var card in fetchedCards)
                {
                    var status = card.SeatsLeft > 0 ? "🎉 AVAILABLE" : "❌ NO SEATS";
                    Console.WriteLine($"[API] {card.CourseCode} Section {card.Section}: {card.SeatsLeft} seats - {status}");
                }
                
                return fetchedCards;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching course data: {ex.Message}");
                throw;
            }
        }
    }
}