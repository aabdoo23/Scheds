using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Scheds.Application.Interfaces.Services;
using Scheds.Domain.Configuration;
using Scheds.Domain.DTOs.SeatAlerts;
using Scheds.Domain.Entities;
using Scheds.Domain.ValueObjects;

namespace Scheds.MVC.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class SeatAlertsController : ControllerBase
    {
        private readonly ISeatAlertService _seatAlertService;
        private readonly IUnsubscribeTokenService _unsubscribeTokenService;
        private readonly FrontendSettings _frontend;

        public SeatAlertsController(
            ISeatAlertService seatAlertService,
            IUnsubscribeTokenService unsubscribeTokenService,
            IOptions<FrontendSettings> frontend)
        {
            _seatAlertService = seatAlertService ?? throw new ArgumentNullException(nameof(seatAlertService));
            _unsubscribeTokenService = unsubscribeTokenService ?? throw new ArgumentNullException(nameof(unsubscribeTokenService));
            _frontend = frontend.Value;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index() => Ok("Seat alerts API is running");

        [HttpGet("~/SeatAlerts")]
        [HttpGet("~/SeatModeration")]
        [AllowAnonymous]
        public IActionResult ViewIndex() =>
            Redirect($"{_frontend.Url.TrimEnd('/')}/seat-alerts");

        /// <summary>The sections the signed-in student is watching. Cheap: no upstream fetch.</summary>
        [HttpGet("watched")]
        [EnableRateLimiting(RateLimitPolicies.SeatAlertsRead)]
        public async Task<IActionResult> GetWatched()
        {
            var userEmail = GetUserEmail();
            if (userEmail == null)
                return Unauthorized(SeatAlertResult.Failed("Authentication required or email not found"));

            try
            {
                var watched = await _seatAlertService.GetUserWatchedSections(userEmail);
                return Ok(new WatchedSectionsResponse
                {
                    Sections = watched
                        .Select(s => new WatchedSectionDto { CourseCode = s.CourseCode, Section = s.Section })
                        .ToList()
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    SeatAlertResult.Failed("Could not load your watched sections"));
            }
        }

        /// <summary>Watched sections with a live seat reading. Triggers an upstream fetch.</summary>
        [HttpGet("watched/live")]
        [EnableRateLimiting(RateLimitPolicies.SeatAlertsLive)]
        public async Task<IActionResult> GetWatchedLive()
        {
            var userEmail = GetUserEmail();
            if (userEmail == null)
                return Unauthorized(SeatAlertResult.Failed("Authentication required or email not found"));

            try
            {
                var readings = await _seatAlertService.GetWatchedSectionsWithSeats(userEmail);
                return Ok(new SeatReadingsResponse
                {
                    CheckedAt = ToIsoUtc(DateTime.UtcNow),
                    Sections = readings.Select(ToReading).ToList()
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    SeatAlertResult.Failed("Could not reach live seat data"));
            }
        }

        [HttpPost("watch")]
        [EnableRateLimiting(RateLimitPolicies.SeatAlertsWrite)]
        public async Task<IActionResult> Watch([FromBody] WatchSectionRequest request)
        {
            var userEmail = GetUserEmail();
            if (userEmail == null)
                return Unauthorized(SeatAlertResult.Failed("Authentication required or email not found"));

            var section = CourseSection.Create(request.CourseCode, request.Section);
            if (section == null)
                return BadRequest(SeatAlertResult.Failed("courseCode and section are required"));

            // Reject a section we can prove does not exist. Subscribing to a code Self-Service has
            // never heard of used to pin a permanent upstream scrape into every monitoring cycle.
            if (await _seatAlertService.CheckSectionExists(section) == SectionExistence.NotFound)
                return NotFound(SeatAlertResult.Failed($"{section} was not found in this term's sections"));

            try
            {
                await _seatAlertService.SubscribeUserToMonitoring(userEmail, new[] { section });
                return Ok(SeatAlertResult.Ok($"Now watching {section}"));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    SeatAlertResult.Failed("Could not start watching that section"));
            }
        }

        [HttpPost("unwatch")]
        [EnableRateLimiting(RateLimitPolicies.SeatAlertsWrite)]
        public async Task<IActionResult> Unwatch([FromBody] WatchSectionRequest request)
        {
            var userEmail = GetUserEmail();
            if (userEmail == null)
                return Unauthorized(SeatAlertResult.Failed("Authentication required or email not found"));

            var section = CourseSection.Create(request.CourseCode, request.Section);
            if (section == null)
                return BadRequest(SeatAlertResult.Failed("courseCode and section are required"));

            try
            {
                await _seatAlertService.UnsubscribeUserFromMonitoring(userEmail, new[] { section });
                return Ok(SeatAlertResult.Ok($"Stopped watching {section}"));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    SeatAlertResult.Failed("Could not stop watching that section"));
            }
        }

        [HttpPost("unwatch-all")]
        [EnableRateLimiting(RateLimitPolicies.SeatAlertsWrite)]
        public async Task<IActionResult> UnwatchAll()
        {
            var userEmail = GetUserEmail();
            if (userEmail == null)
                return Unauthorized(SeatAlertResult.Failed("Authentication required or email not found"));

            try
            {
                await _seatAlertService.UnsubscribeUserFromAll(userEmail);
                return Ok(SeatAlertResult.Ok("Stopped watching every section"));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    SeatAlertResult.Failed("Could not stop watching your sections"));
            }
        }

        /// <summary>
        /// Ad-hoc seat lookup for arbitrary pairs. No longer used by the frontend, which reads
        /// <c>watched/live</c> instead; kept for any external caller. This endpoint was anonymous
        /// and unbounded, which made it an open proxy for scraping the university's portal.
        /// </summary>
        [HttpPost("check-seats")]
        [EnableRateLimiting(RateLimitPolicies.SeatAlertsLive)]
        public async Task<IActionResult> CheckSeats([FromBody] CheckSeatsRequest request)
        {
            // The wire format is two parallel arrays, so the pairing happens here — once, at the
            // edge — instead of being re-derived (and misaligned) further down.
            if (request.CourseCode.Count != request.Sections.Count)
                return BadRequest(SeatAlertResult.Failed("courseCode and sections must have the same length"));

            var requested = new List<CourseSection>();
            for (var i = 0; i < request.CourseCode.Count; i++)
            {
                var section = CourseSection.Create(request.CourseCode[i], request.Sections[i]);
                if (section == null)
                    return BadRequest(SeatAlertResult.Failed($"Invalid course/section pair at index {i}"));
                requested.Add(section);
            }

            try
            {
                var results = await _seatAlertService.FetchAndProcessCourseData(requested);
                return Ok(new SeatReadingsResponse
                {
                    CheckedAt = ToIsoUtc(DateTime.UtcNow),
                    Sections = results.Select(ToReading).ToList()
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    SeatAlertResult.Failed("Could not reach live seat data"));
            }
        }

        /// <summary>
        /// Landing page for the unsubscribe link in an alert email. Deliberately does not mutate
        /// anything: mail clients and security scanners prefetch links, which would silently
        /// unsubscribe people who never clicked. The confirmation posts back to act.
        /// </summary>
        [HttpGet("unsubscribe")]
        // Legacy path: unsubscribe links already sitting in students' inboxes point here.
        [HttpGet("~/api/SeatModeration/unsubscribe")]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicies.SeatAlertsUnsubscribe)]
        public IActionResult UnsubscribeLanding([FromQuery] string? token)
        {
            if (!_unsubscribeTokenService.TryValidate(token, out var userEmail, out var alertId))
                return InvalidTokenPage();

            var scope = DescribeScope(alertId);
            return HtmlPage(
                "Stop seat alerts?",
                $"<p>Stop sending seat alerts for <strong>{Enc(scope)}</strong> to {Enc(userEmail)}?</p>"
                + "<form method=\"post\" action=\"unsubscribe\">"
                + $"<input type=\"hidden\" name=\"token\" value=\"{Enc(token!)}\" />"
                + "<button type=\"submit\">Stop alerts</button>"
                + "</form>");
        }

        [HttpPost("unsubscribe")]
        // Legacy path, so the form on a page reached from an old link posts back successfully.
        [HttpPost("~/api/SeatModeration/unsubscribe")]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicies.SeatAlertsUnsubscribe)]
        public async Task<IActionResult> Unsubscribe([FromForm] string? token)
        {
            if (!_unsubscribeTokenService.TryValidate(token, out var userEmail, out var alertId))
                return InvalidTokenPage();

            if (alertId == null)
            {
                await _seatAlertService.UnsubscribeUserFromAll(userEmail);
            }
            else
            {
                var section = CourseSection.TryParseId(alertId);
                if (section == null)
                    return InvalidTokenPage();

                await _seatAlertService.UnsubscribeUserFromMonitoring(userEmail, new[] { section });
            }

            var manageUrl = $"{_frontend.Url.TrimEnd('/')}/seat-alerts";
            return HtmlPage(
                "Alerts stopped",
                $"<p>You will no longer get seat alerts for <strong>{Enc(DescribeScope(alertId))}</strong>.</p>"
                + $"<p><a href=\"{Enc(manageUrl)}\">Manage your watched sections</a></p>");
        }

        private static SeatReadingDto ToReading(CardItem card) => new()
        {
            CourseCode = card.CourseCode,
            CourseName = card.CourseName,
            Section = card.Section,
            Instructor = card.Instructor,
            HasSeats = card.SeatsLeft > 0,
            SeatsLeft = card.SeatsLeft,
            LastUpdate = ToIsoUtc(card.LastUpdate)
        };

        /// <summary>
        /// Serializes a stored timestamp as unambiguous ISO-8601 UTC. Seat timestamps are written
        /// as UtcNow but come back from SQL with an unspecified kind, and the previous
        /// "yyyy-MM-dd HH:mm:ss" format carried no marker at all — so a browser parsed a UTC value
        /// as local time and computed the wrong data age.
        /// </summary>
        private static string ToIsoUtc(DateTime value) =>
            DateTime.SpecifyKind(value, DateTimeKind.Utc)
                .ToString("o", System.Globalization.CultureInfo.InvariantCulture);

        private static string DescribeScope(string? alertId)
        {
            if (alertId == null)
                return "all sections";

            var section = CourseSection.TryParseId(alertId);
            return section?.ToString() ?? alertId;
        }

        private IActionResult InvalidTokenPage()
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return HtmlPage(
                "Link expired",
                "<p>This unsubscribe link is invalid or has expired.</p>"
                + $"<p><a href=\"{Enc($"{_frontend.Url.TrimEnd('/')}/seat-alerts")}\">"
                + "Manage your watched sections</a></p>");
        }

        private ContentResult HtmlPage(string title, string bodyHtml)
        {
            var html = $$"""
                <!doctype html>
                <html lang="en"><head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1" />
                <meta name="robots" content="noindex" />
                <title>{{Enc(title)}} · Scheds</title>
                <style>
                  :root { color-scheme: dark; }
                  body { margin: 0; min-height: 100vh; display: grid; place-items: center;
                         background: #0d101c; color: #e6edf3; padding: 1.5rem;
                         font-family: system-ui, -apple-system, "Segoe UI", sans-serif; }
                  main { max-width: 26rem; background: #161b22; border: 1px solid #ffffff1a;
                         border-radius: 12px; padding: 2rem; }
                  h1 { margin: 0 0 .75rem; font-size: 1.25rem; }
                  p { margin: 0 0 1rem; color: #9198a1; line-height: 1.5; }
                  strong { color: #e6edf3; }
                  a { color: #58a6ff; }
                  button { min-height: 44px; padding: 0 1.25rem; border: 0; border-radius: 8px;
                           background: #004060; color: #fff; font: inherit; font-weight: 600;
                           cursor: pointer; }
                  button:hover { background: #005a85; }
                </style>
                </head><body><main>
                <h1>{{Enc(title)}}</h1>
                {{bodyHtml}}
                </main></body></html>
                """;

            return Content(html, "text/html; charset=utf-8");
        }

        private static string Enc(string value) => System.Net.WebUtility.HtmlEncode(value);

        private string? GetUserEmail()
        {
            if (User?.Identity?.IsAuthenticated != true)
                return null;

            return User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        }
    }
}
