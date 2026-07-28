using Scheds.Domain.Entities;
using Scheds.Domain.ValueObjects;
using System.Threading;

namespace Scheds.Application.Interfaces.Services
{
    public interface ISeatAlertService
    {
        /// <summary>Current seat data for the given course-section pairs.</summary>
        Task<List<CardItem>> FetchAndProcessCourseData(IReadOnlyCollection<CourseSection> requested);

        Task MonitorAllCourses(CancellationToken cancellationToken);

        /// <summary>
        /// Starts watching the given sections for a user. This is the single write path for
        /// "user wants seat alerts for this section" — there is no separate cart store.
        /// </summary>
        Task SubscribeUserToMonitoring(string userEmail, IReadOnlyCollection<CourseSection> sections);

        Task UnsubscribeUserFromMonitoring(string userEmail, IReadOnlyCollection<CourseSection> sections);

        Task UnsubscribeUserFromAll(string userEmail);

        /// <summary>The sections a user is watching, derived from their subscriptions.</summary>
        Task<List<CourseSection>> GetUserWatchedSections(string userEmail);

        /// <summary>The sections a user is watching, with current seat data attached.</summary>
        Task<List<CardItem>> GetWatchedSectionsWithSeats(string userEmail);

        /// <summary>
        /// Whether a section is known to exist, so callers can reject a provably wrong subscription
        /// without rejecting legitimate ones when there is no stored data to judge by.
        /// </summary>
        Task<SectionExistence> CheckSectionExists(CourseSection section);
    }
}
