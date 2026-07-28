using Microsoft.EntityFrameworkCore;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Domain.Entities;
using Scheds.Infrastructure.Contexts;
using Scheds.Infrastructure.Repositories.Common;

namespace Scheds.Infrastructure.Repositories
{
    public class SeatAlertRepository(SchedsDbContext context)
        : BaseRepository<SeatAlert>(context), ISeatAlertRepository
    {
        private readonly SchedsDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<List<SeatAlert>> GetAllWithSubscriptionsAsync()
        {
            return await _context.SeatAlerts
                .Include(sm => sm.Subscriptions)
                    .ThenInclude(s => s.User)
                .ToListAsync();
        }

        public async Task<List<string>> GetUserModerationIdsAsync(string userEmail)
        {
            return await _context.SeatAlerts
                .Where(sm => sm.Subscriptions.Any(s => s.User.Email == userEmail))
                .Select(sm => sm.Id)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> EnsureUserSubscribedAsync(string moderationId, User user)
        {
            // Two attempts: the second one only runs if we lost a create race to another request.
            for (var attempt = 0; attempt < 2; attempt++)
            {
                var moderation = await GetByIdWithSubscriptionsAsync(moderationId);

                if (moderation != null)
                {
                    if (moderation.Subscriptions.Any(s => s.UserId == user.Id))
                        return false;

                    // Adding to the tracked collection inserts only the join row. Calling Update()
                    // here instead would mark the whole graph modified and rewrite every watcher's
                    // Users row on each subscribe.
                    moderation.Subscriptions.Add(NewSubscription(user));
                    await _context.SaveChangesAsync();
                    return true;
                }

                // LastSeenSeats stays null so the first observation counts as an opening for this
                // user even if the section is already open.
                var created = new SeatAlert(moderationId) { LastCheckedAt = DateTime.UtcNow };
                created.Subscriptions.Add(NewSubscription(user));
                _context.SeatAlerts.Add(created);

                try
                {
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateException)
                {
                    // Another request inserted this section first. Drop our failed insert so the
                    // next SaveChanges does not retry it, then join the existing row.
                    Detach(created);
                }
            }

            return false;
        }

        public async Task RemoveUserSubscriptionAsync(string moderationId, string userId)
        {
            var moderation = await GetByIdWithSubscriptionsAsync(moderationId);
            if (moderation == null)
                return;

            var subscription = moderation.Subscriptions.FirstOrDefault(s => s.UserId == userId);
            if (subscription == null && moderation.Subscriptions.Count > 0)
                return;

            if (subscription != null)
                moderation.Subscriptions.Remove(subscription);

            if (moderation.Subscriptions.Count == 0)
                _context.SeatAlerts.Remove(moderation);

            await _context.SaveChangesAsync();
        }

        public async Task<int> RemoveAllUserSubscriptionsAsync(string userId)
        {
            var moderations = await _context.SeatAlerts
                .Include(sm => sm.Subscriptions)
                .Where(sm => sm.Subscriptions.Any(s => s.UserId == userId))
                .ToListAsync();

            if (moderations.Count == 0)
                return 0;

            foreach (var moderation in moderations)
            {
                var subscription = moderation.Subscriptions.FirstOrDefault(s => s.UserId == userId);
                if (subscription != null)
                    moderation.Subscriptions.Remove(subscription);

                if (moderation.Subscriptions.Count == 0)
                    _context.SeatAlerts.Remove(moderation);
            }

            await _context.SaveChangesAsync();
            return moderations.Count;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        private async Task<SeatAlert?> GetByIdWithSubscriptionsAsync(string id)
        {
            return await _context.SeatAlerts
                .Include(sm => sm.Subscriptions)
                .FirstOrDefaultAsync(sm => sm.Id == id);
        }

        private static SeatAlertSubscription NewSubscription(User user) => new()
        {
            UserId = user.Id,
            User = user,
            // Null = never notified, so this user is eligible on the next cycle even if the
            // section is already open and other watchers were alerted long ago.
            LastNotifiedAt = null
        };

        private void Detach(SeatAlert moderation)
        {
            foreach (var subscription in moderation.Subscriptions)
                _context.Entry(subscription).State = EntityState.Detached;

            _context.Entry(moderation).State = EntityState.Detached;
        }
    }
}
