using Scheds.Application.Interfaces.Repositories;
using Scheds.Application.Interfaces.Services;
using Scheds.Domain.Entities;
using Scheds.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace Scheds.Infrastructure.Services
{
    public class SeatModerationService(ISelfServiceLiveFetchService selfServiceLiveFetchService,
        SchedsDbContext context,
        IEmailService emailService) : ISeatModerationService
    {
        private readonly ISelfServiceLiveFetchService _selfServiceLiveFetchService = selfServiceLiveFetchService
            ?? throw new ArgumentNullException(nameof(selfServiceLiveFetchService));
        private readonly SchedsDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));

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

        public async Task MoniterAllCourses(CancellationToken cancellationToken)
        {
            try
            {

                var seatModerations = await _context.Set<SeatModeration>()
                    .Include(sm => sm.Users)
                    .ToListAsync(cancellationToken);

                if (!seatModerations.Any())
                {
                    Console.WriteLine("[MONITOR] No seat moderations found, skipping cycle");
                    return;
                }


                var courseCodeSectionPairs = seatModerations
                    .Select(sm => sm.CourseCode_Section.Split('_'))
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

                    var parts = seatModeration.CourseCode_Section.Split('_');
                    if (parts.Length != 2) continue;

                    var courseCode = parts[0];
                    var section = parts[1];

                    var matchingCard = allCards.FirstOrDefault(card =>
                        card.CourseCode.Equals(courseCode, StringComparison.OrdinalIgnoreCase) &&
                        card.Section.PadLeft(2, '0').Equals(section.PadLeft(2, '0'), StringComparison.OrdinalIgnoreCase));

                    if (matchingCard != null && matchingCard.SeatsLeft > 0)
                    {
                        Console.WriteLine($"[MONITOR] 🎉 Seats available for {courseCode} Section {section}: {matchingCard.SeatsLeft} seats");

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
                    }
                    else if (matchingCard != null)
                    {
                        continue;
                    }
                    else
                    {
                        Console.WriteLine($"[MONITOR] ⚠️ No data found for {courseCode} Section {section}");
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
                        Console.WriteLine($"[EMAIL] ✅ Sent summary to {email}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[EMAIL] ❌ Failed to send summary to {email}: {ex.Message}");
                    }
                }

                Console.WriteLine("[MONITOR] Seat monitoring cycle completed");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[MONITOR] Monitoring cycle cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MONITOR] Error during monitoring cycle: {ex.Message}");
                throw;
            }
        }
        

        public async Task SubscribeUserToMonitoring(string userEmail, List<string> courseSections)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user == null)
                {
                    user = new User { Email = userEmail };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                foreach (var courseSection in courseSections)
                {
                    var seatModeration = await _context.SeatModerations
                        .Include(sm => sm.Users)
                        .FirstOrDefaultAsync(sm => sm.CourseCode_Section == courseSection);

                    if (seatModeration == null)
                    {
                        seatModeration = new SeatModeration 
                        { 
                            CourseCode_Section = courseSection,
                            Users = new List<User>()
                        };
                        _context.SeatModerations.Add(seatModeration);
                    }

                    if (!seatModeration.Users.Any(u => u.Email == userEmail))
                    {
                        seatModeration.Users.Add(user);
                    }
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"[SUBSCRIPTION] User {userEmail} subscribed to monitoring for {courseSections.Count} course sections");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SUBSCRIPTION] Error subscribing user to monitoring: {ex.Message}");
                throw;
            }
        }

        public async Task UnsubscribeUserFromMonitoring(string userEmail, List<string> courseSections)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user == null)
                {
                    Console.WriteLine($"[UNSUBSCRIPTION] User {userEmail} not found");
                    return;
                }

                foreach (var courseSection in courseSections)
                {
                    var seatModeration = await _context.SeatModerations
                        .Include(sm => sm.Users)
                        .FirstOrDefaultAsync(sm => sm.CourseCode_Section == courseSection);

                    if (seatModeration != null)
                    {
                        var userToRemove = seatModeration.Users.FirstOrDefault(u => u.Email == userEmail);
                        if (userToRemove != null)
                        {
                            seatModeration.Users.Remove(userToRemove);
                        }

                        if (!seatModeration.Users.Any())
                        {
                            _context.SeatModerations.Remove(seatModeration);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"[UNSUBSCRIPTION] User {userEmail} unsubscribed from monitoring for {courseSections.Count} course sections");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UNSUBSCRIPTION] Error unsubscribing user from monitoring: {ex.Message}");
                throw;
            }
        }

        public async Task AddToSeatModerationCart(string userEmail, string courseCode, string section)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user == null)
                {
                    user = new User { Email = userEmail };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                var existingCartItem = await _context.CartSeatModerations
                    .FirstOrDefaultAsync(c => c.UserId == user.Id && c.CourseCode == courseCode && c.Section == section);

                if (existingCartItem == null)
                {
                    var cartItem = new CartSeatModeration
                    {
                        UserId = user.Id,
                        CourseCode = courseCode,
                        Section = section,
                        User = user
                    };

                    _context.CartSeatModerations.Add(cartItem);
                    await _context.SaveChangesAsync();
                    
                    var courseSection = $"{courseCode}_{section}";
                    await SubscribeUserToMonitoring(userEmail, new List<string> { courseSection });
                    
                    Console.WriteLine($"[CART] Added {courseCode} Section {section} to cart and auto-monitoring for user {userEmail}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CART] Error adding to cart: {ex.Message}");
                throw;
            }
        }

        public async Task RemoveFromSeatModerationCart(string userEmail, string courseCode, string section)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user == null) return;

                var cartItem = await _context.CartSeatModerations
                    .FirstOrDefaultAsync(c => c.UserId == user.Id && c.CourseCode == courseCode && c.Section == section);

                if (cartItem != null)
                {
                    _context.CartSeatModerations.Remove(cartItem);
                    await _context.SaveChangesAsync();
                    
                    var courseSection = $"{courseCode}_{section}";
                    await UnsubscribeUserFromMonitoring(userEmail, new List<string> { courseSection });
                    
                    Console.WriteLine($"[CART] Removed {courseCode} Section {section} from cart and auto-monitoring for user {userEmail}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CART] Error removing from cart: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CartSeatModeration>> GetSeatModerationCart(string userEmail)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user == null) return new List<CartSeatModeration>();

                var cartItems = await _context.CartSeatModerations
                    .Where(c => c.UserId == user.Id)
                    .Include(c => c.User)
                    .ToListAsync();

                return cartItems;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CART] Error getting cart: {ex.Message}");
                throw;
            }
        }

        public async Task ClearSeatModerationCart(string userEmail)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user == null) return;

                var cartItems = await _context.CartSeatModerations
                    .Where(c => c.UserId == user.Id)
                    .ToListAsync();

                if (cartItems.Any())
                {
                    var courseSections = cartItems
                        .Select(c => $"{c.CourseCode}_{c.Section}")
                        .ToList();
                    
                    _context.CartSeatModerations.RemoveRange(cartItems);
                    await _context.SaveChangesAsync();
                    
                    await UnsubscribeUserFromMonitoring(userEmail, courseSections);
                    
                    Console.WriteLine($"[CART] Cleared cart and unsubscribed from auto-monitoring for {courseSections.Count} courses for user {userEmail}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CART] Error clearing cart: {ex.Message}");
                throw;
            }
        }
    }
}