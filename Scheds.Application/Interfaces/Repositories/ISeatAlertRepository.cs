using Scheds.Application.Interfaces.Repositories.Common;
using Scheds.Domain.Entities;

namespace Scheds.Application.Interfaces.Repositories
{
    public interface ISeatAlertRepository : IBaseRepository<SeatAlert>
    {
        /// <summary>
        /// Every watched section with its subscriptions and their users attached — one monitoring
        /// cycle's working set.
        /// </summary>
        Task<List<SeatAlert>> GetAllWithSubscriptionsAsync();

        /// <summary>
        /// Ids of the sections a user watches, without loading the subscription graph.
        /// </summary>
        Task<List<string>> GetUserModerationIdsAsync(string userEmail);

        /// <summary>
        /// Subscribes a user to a section, creating the section if nobody watches it yet. Safe
        /// against two requests racing to create the same one. Returns true if this call added the
        /// subscription, false if it was already there.
        /// </summary>
        Task<bool> EnsureUserSubscribedAsync(string moderationId, User user);

        /// <summary>
        /// Removes one subscription, deleting the section once nobody is watching it.
        /// </summary>
        Task RemoveUserSubscriptionAsync(string moderationId, string userId);

        /// <summary>
        /// Removes every subscription belonging to a user in a single round trip, deleting any
        /// section left with no watchers. Returns the number of subscriptions removed.
        /// </summary>
        Task<int> RemoveAllUserSubscriptionsAsync(string userId);

        /// <summary>Persists pending changes to already-tracked entities in one round trip.</summary>
        Task SaveChangesAsync();
    }
}
