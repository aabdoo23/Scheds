using Microsoft.Extensions.Logging;
using Scheds.Application.Interfaces.Services;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Domain.Entities;

namespace Scheds.Infrastructure.Services
{
    public class SeatModerationService(
        ILogger<SeatModerationService> logger,
        ISelfServiceLiveFetchService selfServiceLiveFetchService,
        ISeatModerationRepository seatModerationRepository,
        ICartSeatModerationRepository cartSeatModerationRepository,
        IUserRepository userRepository,
        IEmailService emailService) : ISeatModerationService
    {
        private readonly ILogger<SeatModerationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ISelfServiceLiveFetchService _selfServiceLiveFetchService = selfServiceLiveFetchService
            ?? throw new ArgumentNullException(nameof(selfServiceLiveFetchService));
        private readonly ISeatModerationRepository _seatModerationRepository = seatModerationRepository
            ?? throw new ArgumentNullException(nameof(seatModerationRepository));
        private readonly ICartSeatModerationRepository _cartSeatModerationRepository = cartSeatModerationRepository
            ?? throw new ArgumentNullException(nameof(cartSeatModerationRepository));
        private readonly IUserRepository _userRepository = userRepository
            ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));

        public async Task<List<CardItem>> FetchAndProcessCourseData(List<string> courseCodes, List<string> sections)
        {
            try
            {
                var fetchedCards = await _selfServiceLiveFetchService.FetchCardsSeatModeration(courseCodes, sections);
                _logger.LogInformation("Fetched {Count} courses for frontend", fetchedCards.Count);
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

                var seatModerations = await _seatModerationRepository.GetAllWithUsersAsync();

                if (!seatModerations.Any())
                {
                    Console.WriteLine("[MONITOR] No seat moderations found, skipping cycle");
                    return;
                }


                var courseCodeSectionPairs = seatModerations
                    .Select(sm => sm.Id.Split('_'))
                    .Where(parts => parts.Length == 2)
                    .Select(parts => new { CourseCode = parts[0], Section = parts[1] })
                    .ToList();

                var uniqueCourseCodes = courseCodeSectionPairs
                    .Select(pair => pair.CourseCode)
                    .Distinct()
                    .ToList();

                var allSections = courseCodeSectionPairs
                    .Select(pair => pair.Section)
                    .Distinct()
                    .ToList();

                var allCards = await FetchAndProcessCourseData(uniqueCourseCodes, allSections);


                var userNotifications = new Dictionary<string, List<(string CourseCode, string Section, int SeatsLeft)>>();

                foreach (var seatModeration in seatModerations)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var parts = seatModeration.Id.Split('_');
                    if (parts.Length != 2) continue;

                    var courseCode = parts[0];
                    var section = parts[1];

                    var matchingCard = allCards.FirstOrDefault(card =>
                        card.CourseCode.Equals(courseCode, StringComparison.OrdinalIgnoreCase) &&
                        card.Section.PadLeft(2, '0').Equals(section.PadLeft(2, '0'), StringComparison.OrdinalIgnoreCase));

                    if (matchingCard != null && matchingCard.SeatsLeft > 0)
                    {
                        var minutesSinceLastEmail = (DateTime.UtcNow - seatModeration.LastUpdated).TotalMinutes;
                        var cooldownPeriod = 15;

                        if (minutesSinceLastEmail >= cooldownPeriod)
                        {

                            foreach (var user in seatModeration.Users)
                            {
                                var email = user.Email?.Trim();
                                if (!string.IsNullOrWhiteSpace(email))
                                {
                                    if (!userNotifications.ContainsKey(email))
                                        userNotifications[email] = new List<(string, string, int)>();
                                    userNotifications[email].Add((courseCode, section, matchingCard.SeatsLeft));
                                }
                            }

                            seatModeration.LastUpdated = DateTime.UtcNow;
                        }
                        else
                        {
                            var remainingCooldown = cooldownPeriod - minutesSinceLastEmail;
                            _logger.LogDebug("Seats available for {CourseCode} Section {Section} but cooldown active (remaining: {Remaining:F1} min)", courseCode, section, remainingCooldown);
                        }
                    }
                    else if (matchingCard != null)
                    {
                        continue;
                    }
                    else
                    {
                        _logger.LogWarning("No data found for {CourseCode} Section {Section}", courseCode, section);
                    }
                }
                foreach (var kvp in userNotifications)
                {
                    var email = kvp.Key;
                    var notifications = kvp.Value;
                    var subject = $"Seats Available: {notifications.Count} Course(s)";
                    var htmlBody = "<h2>Good News! Seats are now available in the following courses:</h2><ul>";
                    foreach (var n in notifications)
                    {
                        htmlBody += $"<li><strong>Course:</strong> {n.CourseCode} <strong>Section:</strong> {n.Section} <strong>Available Seats:</strong> {n.SeatsLeft}</li>";
                    }
                    htmlBody += "</ul><hr><small>This is an automated notification from the Scheds system.</small>";
                    try
                    {
                        await _emailService.SendEmailAsync(email, subject, htmlBody);
                        _logger.LogInformation("Sent seat availability summary to {Email}", email);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send summary to {Email}", email);
                    }
                }

                // Update timestamps for seat moderations that sent notifications
                if (userNotifications.Any())
                {
                    foreach (var seatMod in seatModerations.Where(sm => sm.LastUpdated > DateTime.UtcNow.AddMinutes(-1)))
                    {
                        await _seatModerationRepository.UpdateAsync(seatMod);
                    }
                    _logger.LogInformation("Updated cooldown timestamps for {Count} notification(s)", userNotifications.Count);
                }

                _logger.LogDebug("Seat monitoring cycle completed");
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
        

        public async Task SubscribeUserToMonitoring(string userEmail, List<string> courseSections)
        {
            try
            {
                var user = await _userRepository.GetOrCreateUserAsync(userEmail);

                foreach (var courseSection in courseSections)
                {
                    var seatModeration = await _seatModerationRepository.GetByIdWithUsersAsync(courseSection);

                    if (seatModeration == null)
                    {
                        seatModeration = new SeatModeration(courseSection)
                        { 
                            Users = new List<User>(),
                            LastUpdated = DateTime.UtcNow.AddMinutes(-20) // Set to 20 minutes ago to allow immediate email sending
                        };
                        await _seatModerationRepository.InsertAsync(seatModeration);
                    }

                    if (!seatModeration.Users.Any(u => u.Email == userEmail))
                    {
                        seatModeration.Users.Add(user);
                        await _seatModerationRepository.UpdateAsync(seatModeration);
                    }
                }

                _logger.LogInformation("User {Email} subscribed to monitoring for {Count} course sections", userEmail, courseSections.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing user to monitoring");
                throw;
            }
        }

        public async Task UnsubscribeUserFromMonitoring(string userEmail, List<string> courseSections)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(userEmail);
                if (user == null)
                {
                    _logger.LogWarning("User {Email} not found for unsubscription", userEmail);
                    return;
                }

                foreach (var courseSection in courseSections)
                {
                    var seatModeration = await _seatModerationRepository.GetByIdWithUsersAsync(courseSection);

                    if (seatModeration != null)
                    {
                        var userToRemove = seatModeration.Users.FirstOrDefault(u => u.Email == userEmail);
                        if (userToRemove != null)
                        {
                            seatModeration.Users.Remove(userToRemove);
                        }

                        if (!seatModeration.Users.Any())
                        {
                            await _seatModerationRepository.DeleteAsync(seatModeration.Id);
                        }
                        else
                        {
                            await _seatModerationRepository.UpdateAsync(seatModeration);
                        }
                    }
                }

                _logger.LogInformation("User {Email} unsubscribed from monitoring for {Count} course sections", userEmail, courseSections.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing user from monitoring");
                throw;
            }
        }

        public async Task AddToSeatModerationCart(string userEmail, string courseCode, string section)
        {
            try
            {
                var user = await _userRepository.GetOrCreateUserAsync(userEmail);

                var existingCartItem = await _cartSeatModerationRepository.GetByUserAndCourseAsync(user.Id, courseCode, section);

                if (existingCartItem == null)
                {
                    var cartItem = new CartSeatModeration
                    {
                        Id = $"{user.Id}_{courseCode}_{section}", // Set unique composite key
                        UserId = user.Id,
                        CourseCode = courseCode,
                        Section = section,
                        User = user
                    };

                    await _cartSeatModerationRepository.InsertAsync(cartItem);
                    
                    var courseSection = $"{courseCode}_{section}";
                    await SubscribeUserToMonitoring(userEmail, new List<string> { courseSection });
                    _logger.LogInformation("Added {CourseCode} Section {Section} to cart for user {Email}", courseCode, section, userEmail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to cart");
                throw;
            }
        }

        public async Task RemoveFromSeatModerationCart(string userEmail, string courseCode, string section)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(userEmail);
                if (user == null) return;

                var cartItem = await _cartSeatModerationRepository.GetByUserAndCourseAsync(user.Id, courseCode, section);

                if (cartItem != null)
                {
                    await _cartSeatModerationRepository.DeleteAsync(cartItem.Id);
                    
                    var courseSection = $"{courseCode}_{section}";
                    await UnsubscribeUserFromMonitoring(userEmail, new List<string> { courseSection });
                    _logger.LogInformation("Removed {CourseCode} Section {Section} from cart for user {Email}", courseCode, section, userEmail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing from cart");
                throw;
            }
        }

        public async Task<List<CartSeatModeration>> GetSeatModerationCart(string userEmail)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(userEmail);
                if (user == null) return new List<CartSeatModeration>();

                var cartItems = await _cartSeatModerationRepository.GetUserCartItemsAsync(user.Id);

                return cartItems;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart");
                throw;
            }
        }

        public async Task ClearSeatModerationCart(string userEmail)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(userEmail);
                if (user == null) return;

                var cartItems = await _cartSeatModerationRepository.GetUserCartItemsAsync(user.Id);

                if (cartItems.Any())
                {
                    var courseSections = cartItems
                        .Select(c => $"{c.CourseCode}_{c.Section}")
                        .ToList();
                    
                    await _cartSeatModerationRepository.ClearUserCartAsync(user.Id);
                    
                    await UnsubscribeUserFromMonitoring(userEmail, courseSections);
                    _logger.LogInformation("Cleared cart for user {Email}, unsubscribed from {Count} courses", userEmail, courseSections.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                throw;
            }
        }

        public async Task<List<CardItem>> GetUserActiveMonitoringJobs(string userEmail)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(userEmail);
                if (user == null)
                {
                    _logger.LogWarning("User {Email} not found for monitoring jobs", userEmail);
                    return new List<CardItem>();
                }

                var userSeatModerations = await _seatModerationRepository.GetUserSeatModerationsAsync(userEmail);

                if (!userSeatModerations.Any())
                {
                    return new List<CardItem>();
                }

                // Extract course codes and sections from the monitoring jobs
                var courseCodeSectionPairs = userSeatModerations
                    .Select(sm => sm.Id.Split('_'))
                    .Where(parts => parts.Length == 2)
                    .Select(parts => new { CourseCode = parts[0], Section = parts[1] })
                    .ToList();

                var uniqueCourseCodes = courseCodeSectionPairs
                    .Select(pair => pair.CourseCode)
                    .Distinct()
                    .ToList();

                var allSections = courseCodeSectionPairs
                    .Select(pair => pair.Section)
                    .Distinct()
                    .ToList();

                // Fetch current seat data for these courses
                var currentSeatData = await FetchAndProcessCourseData(uniqueCourseCodes, allSections);

                // Filter to only include the exact course-section combinations the user is monitoring
                var userMonitoredCourses = currentSeatData.Where(card =>
                    courseCodeSectionPairs.Any(pair =>
                        card.CourseCode.Equals(pair.CourseCode, StringComparison.OrdinalIgnoreCase) &&
                        card.Section.PadLeft(2, '0').Equals(pair.Section.PadLeft(2, '0'), StringComparison.OrdinalIgnoreCase)
                    )
                ).ToList();

                return userMonitoredCourses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user active monitoring jobs");
                throw;
            }
        }
    }
}