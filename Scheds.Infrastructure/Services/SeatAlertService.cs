using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scheds.Application.Interfaces.Services;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Domain.Configuration;
using Scheds.Domain.Entities;
using Scheds.Domain.ValueObjects;
using System.Net;
using System.Text;

namespace Scheds.Infrastructure.Services
{
    public class SeatAlertService(
        ILogger<SeatAlertService> logger,
        ISelfServiceLiveFetchService selfServiceLiveFetchService,
        ISeatAlertRepository seatAlertRepository,
        ICardItemRepository cardItemRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        IUnsubscribeTokenService unsubscribeTokenService,
        IOptions<SeatAlertSettings> seatAlertSettings) : ISeatAlertService
    {
        /// <summary>
        /// Floor between two alerts for the same user and section. Alerts are edge-triggered, so
        /// this is not a general cooldown — it only guards against Self-Service flapping a section
        /// open and shut repeatedly.
        /// </summary>
        private static readonly TimeSpan ReNotifyFloor = TimeSpan.FromMinutes(15);

        private readonly ILogger<SeatAlertService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ISelfServiceLiveFetchService _selfServiceLiveFetchService = selfServiceLiveFetchService
            ?? throw new ArgumentNullException(nameof(selfServiceLiveFetchService));
        private readonly ISeatAlertRepository _seatAlertRepository = seatAlertRepository
            ?? throw new ArgumentNullException(nameof(seatAlertRepository));
        private readonly ICardItemRepository _cardItemRepository = cardItemRepository
            ?? throw new ArgumentNullException(nameof(cardItemRepository));
        private readonly IUserRepository _userRepository = userRepository
            ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        private readonly IUnsubscribeTokenService _unsubscribeTokenService = unsubscribeTokenService
            ?? throw new ArgumentNullException(nameof(unsubscribeTokenService));
        private readonly SeatAlertSettings _seatAlertSettings = seatAlertSettings?.Value
            ?? throw new ArgumentNullException(nameof(seatAlertSettings));

        /// <summary>One user who should be told that one section has opened.</summary>
        private sealed record SeatOpening(SeatAlertSubscription Subscription, CourseSection Section, int SeatsLeft);

        public async Task<List<CardItem>> FetchAndProcessCourseData(IReadOnlyCollection<CourseSection> requested)
        {
            try
            {
                var fetchedCards = await _selfServiceLiveFetchService.FetchSeatData(requested);
                _logger.LogInformation("Fetched {Count} card(s) for {Requested} requested section(s)",
                    fetchedCards.Count, requested.Count);
                return fetchedCards;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching course data");
                throw;
            }
        }

        public async Task MonitorAllCourses(CancellationToken cancellationToken)
        {
            try
            {
                var seatAlerts = await _seatAlertRepository.GetAllWithSubscriptionsAsync();
                if (seatAlerts.Count == 0)
                {
                    _logger.LogDebug("No watched sections found, skipping cycle");
                    return;
                }

                var watched = new List<(SeatAlert Moderation, CourseSection Section)>();
                foreach (var moderation in seatAlerts)
                {
                    var section = CourseSection.TryParseId(moderation.Id);
                    if (section == null)
                    {
                        _logger.LogWarning("Skipping watched section with unparseable id {Id}", moderation.Id);
                        continue;
                    }
                    watched.Add((moderation, section));
                }

                if (watched.Count == 0)
                    return;

                // Pass real pairs. Sending distinct course codes and distinct sections as two
                // parallel lists used to zip them positionally, so most subscriptions were never
                // actually checked and never notified.
                var requested = watched.Select(w => w.Section).Distinct().ToList();
                var cardsBySection = IndexBySection(await FetchAndProcessCourseData(requested));

                var now = DateTime.UtcNow;
                var openings = new List<SeatOpening>();

                foreach (var (moderation, section) in watched)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!cardsBySection.TryGetValue(section, out var card))
                    {
                        _logger.LogWarning("No live data found for {Section}", section);
                        continue;
                    }

                    if (moderation.RecordObservation(card.SeatsLeft, now))
                        _logger.LogInformation("{Section} opened with {Seats} seat(s)", section, card.SeatsLeft);

                    if (card.SeatsLeft <= 0)
                        continue;

                    foreach (var subscription in moderation.Subscriptions)
                    {
                        if (subscription.ShouldNotify(moderation.OpenSince, now, ReNotifyFloor))
                            openings.Add(new SeatOpening(subscription, section, card.SeatsLeft));
                    }
                }

                if (openings.Count > 0)
                    await SendAlerts(openings);

                // Persist observations every cycle, whether or not anything was sent — the
                // open/closed edge is what triggers the next alert.
                await _seatAlertRepository.SaveChangesAsync();

                _logger.LogDebug("Seat monitoring cycle completed, {Count} alert(s) queued", openings.Count);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Monitoring cycle cancelled");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during monitoring cycle");
                throw;
            }
        }

        private async Task SendAlerts(List<SeatOpening> openings)
        {
            foreach (var (email, items) in GroupByRecipient(openings))
            {
                try
                {
                    await _emailService.SendEmailAsync(email, BuildSubject(items), BuildBody(email, items));

                    // Stamp only after a successful send. Stamping first meant one SMTP failure
                    // silently swallowed the alert.
                    var sentAt = DateTime.UtcNow;
                    foreach (var item in items)
                        item.Subscription.LastNotifiedAt = sentAt;

                    _logger.LogInformation("Sent seat alert for {Count} section(s) to {Email}", items.Count, email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send seat alert to {Email}", email);
                }
            }
        }

        public async Task SubscribeUserToMonitoring(string userEmail, IReadOnlyCollection<CourseSection> sections)
        {
            if (sections.Count == 0)
                return;

            try
            {
                var user = await _userRepository.GetOrCreateUserAsync(userEmail);

                var added = 0;
                foreach (var section in sections.Distinct())
                {
                    if (await _seatAlertRepository.EnsureUserSubscribedAsync(section.ToId(), user))
                        added++;
                }

                _logger.LogInformation("User {Email} subscribed to {Added} new section(s) of {Requested} requested",
                    userEmail, added, sections.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing user to monitoring");
                throw;
            }
        }

        public async Task UnsubscribeUserFromMonitoring(string userEmail, IReadOnlyCollection<CourseSection> sections)
        {
            if (sections.Count == 0)
                return;

            try
            {
                var user = await _userRepository.GetByEmailAsync(userEmail);
                if (user == null)
                {
                    _logger.LogWarning("User {Email} not found for unsubscription", userEmail);
                    return;
                }

                foreach (var section in sections.Distinct())
                    await _seatAlertRepository.RemoveUserSubscriptionAsync(section.ToId(), user.Id);

                _logger.LogInformation("User {Email} unsubscribed from {Count} section(s)", userEmail, sections.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing user from monitoring");
                throw;
            }
        }

        public async Task UnsubscribeUserFromAll(string userEmail)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(userEmail);
                if (user == null)
                    return;

                var removed = await _seatAlertRepository.RemoveAllUserSubscriptionsAsync(user.Id);
                _logger.LogInformation("Cleared {Count} watched section(s) for {Email}", removed, userEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing watched sections");
                throw;
            }
        }

        public async Task<List<CourseSection>> GetUserWatchedSections(string userEmail)
        {
            try
            {
                var moderationIds = await _seatAlertRepository.GetUserModerationIdsAsync(userEmail);

                return moderationIds
                    .Select(CourseSection.TryParseId)
                    .Where(section => section != null)
                    .Select(section => section!)
                    .Distinct()
                    .OrderBy(section => section.CourseCode, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(section => section.Section, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting watched sections");
                throw;
            }
        }

        public async Task<List<CardItem>> GetWatchedSectionsWithSeats(string userEmail)
        {
            try
            {
                var watched = await GetUserWatchedSections(userEmail);
                if (watched.Count == 0)
                    return new List<CardItem>();

                // FetchAndProcessCourseData already returns only the requested pairs.
                return await FetchAndProcessCourseData(watched);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting watched sections with seats");
                throw;
            }
        }

        public async Task<SectionExistence> CheckSectionExists(CourseSection section)
        {
            var cards = await _cardItemRepository.GetSeatDataByCourseCodeAsync(section.CourseCode);
            if (cards.Count == 0)
                return SectionExistence.Unknown;

            var exists = cards.Any(card => section.Equals(CourseSection.Create(card.CourseCode, card.Section)));
            return exists ? SectionExistence.Exists : SectionExistence.NotFound;
        }

        private static Dictionary<CourseSection, CardItem> IndexBySection(IEnumerable<CardItem> cards)
        {
            var index = new Dictionary<CourseSection, CardItem>();
            foreach (var card in cards)
            {
                var section = CourseSection.Create(card.CourseCode, card.Section);
                if (section != null)
                    index[section] = card;
            }
            return index;
        }

        private static Dictionary<string, List<SeatOpening>> GroupByRecipient(IEnumerable<SeatOpening> openings)
        {
            var byRecipient = new Dictionary<string, List<SeatOpening>>(StringComparer.OrdinalIgnoreCase);

            foreach (var opening in openings)
            {
                var email = opening.Subscription.User?.Email?.Trim();
                if (string.IsNullOrWhiteSpace(email))
                    continue;

                if (!byRecipient.TryGetValue(email, out var items))
                    byRecipient[email] = items = new List<SeatOpening>();

                items.Add(opening);
            }

            return byRecipient;
        }

        private static string BuildSubject(List<SeatOpening> openings)
        {
            return openings.Count == 1
                ? $"Seat open: {openings[0].Section.CourseCode} Section {openings[0].Section.Section}"
                : $"Seats open: {openings.Count} sections";
        }

        private string BuildBody(string recipientEmail, List<SeatOpening> openings)
        {
            var body = new StringBuilder();
            body.Append("<h2>A seat just opened up:</h2><ul>");

            foreach (var opening in openings)
            {
                // Course code and section are user-supplied at subscribe time, so encode them.
                body.Append("<li><strong>")
                    .Append(WebUtility.HtmlEncode(opening.Section.CourseCode))
                    .Append("</strong> Section ")
                    .Append(WebUtility.HtmlEncode(opening.Section.Section))
                    .Append(" — ")
                    .Append(opening.SeatsLeft)
                    .Append(opening.SeatsLeft == 1 ? " seat available" : " seats available");

                var sectionLink = BuildUnsubscribeUrl(recipientEmail, opening.Subscription.SeatAlertId);
                if (sectionLink != null)
                    body.Append(" (<a href=\"").Append(sectionLink).Append("\">stop watching this section</a>)");

                body.Append("</li>");
            }

            body.Append("</ul><hr>");

            var allLink = BuildUnsubscribeUrl(recipientEmail, null);
            if (allLink != null)
            {
                body.Append("<p><small><a href=\"").Append(allLink)
                    .Append("\">Stop all seat alerts</a></small></p>");
            }

            body.Append("<small>This is an automated notification from the Scheds system.</small>");
            return body.ToString();
        }

        private string? BuildUnsubscribeUrl(string recipientEmail, string? moderationId)
        {
            var baseUrl = _seatAlertSettings.PublicBaseUrl?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl))
                return null;

            var token = _unsubscribeTokenService.Create(recipientEmail, moderationId);
            if (token == null)
                return null;

            return $"{baseUrl}/api/SeatAlert/unsubscribe?token={Uri.EscapeDataString(token)}";
        }
    }
}
