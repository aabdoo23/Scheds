using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scheds.Application.Interfaces.Services;
using Scheds.Domain.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Scheds.Infrastructure.Services
{
    /// <summary>
    /// Stateless unsubscribe tokens: <c>base64url(payload).base64url(HMAC-SHA256(payload))</c>.
    /// </summary>
    /// <remarks>
    /// Signed rather than stored so a link keeps working across restarts and deploys without a
    /// DB row per email. Data-protection keys would be the other option, but this app persists
    /// them to the temp directory, which a container restart can wipe — and these links sit in
    /// an inbox for weeks.
    /// </remarks>
    public class UnsubscribeTokenService : IUnsubscribeTokenService
    {
        private const string Purpose = "scheds-seat-alert-unsubscribe-v1";
        private const string AllSections = "*";

        private readonly ILogger<UnsubscribeTokenService> _logger;
        private readonly SeatAlertSettings _settings;
        private readonly byte[]? _key;

        public UnsubscribeTokenService(
            IConfiguration config,
            IOptions<SeatAlertSettings> settings,
            ILogger<UnsubscribeTokenService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

            var configuredKey = !string.IsNullOrWhiteSpace(_settings.UnsubscribeKey)
                ? _settings.UnsubscribeKey
                : config["Jwt:Key"];

            _key = TryDecodeKey(configuredKey);
            if (_key == null)
                _logger.LogWarning("No unsubscribe signing key configured; alert emails will omit unsubscribe links");
        }

        public string? Create(string userEmail, string? moderationId)
        {
            if (_key == null || string.IsNullOrWhiteSpace(userEmail))
                return null;

            var expiry = DateTimeOffset.UtcNow.AddDays(Math.Max(1, _settings.UnsubscribeTokenDays));
            var payload = string.Join('|',
                Purpose,
                userEmail.Trim(),
                string.IsNullOrWhiteSpace(moderationId) ? AllSections : moderationId,
                expiry.ToUnixTimeSeconds().ToString());

            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            return $"{ToBase64Url(payloadBytes)}.{ToBase64Url(Sign(payloadBytes))}";
        }

        public bool TryValidate(string? token, out string userEmail, out string? moderationId)
        {
            userEmail = string.Empty;
            moderationId = null;

            if (_key == null || string.IsNullOrWhiteSpace(token))
                return false;

            var separator = token.IndexOf('.');
            if (separator <= 0 || separator == token.Length - 1)
                return false;

            byte[] payloadBytes, signature;
            try
            {
                payloadBytes = FromBase64Url(token[..separator]);
                signature = FromBase64Url(token[(separator + 1)..]);
            }
            catch (FormatException)
            {
                return false;
            }

            if (!CryptographicOperations.FixedTimeEquals(signature, Sign(payloadBytes)))
                return false;

            var parts = Encoding.UTF8.GetString(payloadBytes).Split('|');
            if (parts.Length != 4 || parts[0] != Purpose)
                return false;

            if (!long.TryParse(parts[3], out var expiryUnix)
                || DateTimeOffset.FromUnixTimeSeconds(expiryUnix) < DateTimeOffset.UtcNow)
                return false;

            if (string.IsNullOrWhiteSpace(parts[1]))
                return false;

            userEmail = parts[1];
            moderationId = parts[2] == AllSections ? null : parts[2];
            return true;
        }

        private byte[] Sign(byte[] payload)
        {
            using var hmac = new HMACSHA256(_key!);
            return hmac.ComputeHash(payload);
        }

        private static byte[]? TryDecodeKey(string? configured)
        {
            if (string.IsNullOrWhiteSpace(configured))
                return null;

            // The JWT key is base64; tolerate a raw string too so a dedicated key can be plain text.
            try
            {
                var decoded = Convert.FromBase64String(configured);
                if (decoded.Length > 0)
                    return decoded;
            }
            catch (FormatException)
            {
                // fall through
            }

            return Encoding.UTF8.GetBytes(configured);
        }

        private static string ToBase64Url(byte[] value) =>
            Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        private static byte[] FromBase64Url(string value)
        {
            var padded = value.Replace('-', '+').Replace('_', '/');
            padded += (padded.Length % 4) switch
            {
                2 => "==",
                3 => "=",
                0 => "",
                _ => throw new FormatException("Invalid base64url length")
            };
            return Convert.FromBase64String(padded);
        }
    }
}
