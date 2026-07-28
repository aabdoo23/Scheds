namespace Scheds.Application.Interfaces.Services
{
    /// <summary>
    /// Issues and verifies the signed tokens that let a seat-alert email unsubscribe its
    /// recipient without them being signed in.
    /// </summary>
    public interface IUnsubscribeTokenService
    {
        /// <summary>
        /// Creates a token for one section, or for every section when
        /// <paramref name="moderationId"/> is null. Returns null when no signing key is
        /// configured, in which case callers should omit the link rather than fail.
        /// </summary>
        string? Create(string userEmail, string? moderationId);

        /// <summary>
        /// Verifies a token's signature and expiry. <paramref name="moderationId"/> is null for
        /// an unsubscribe-from-everything token.
        /// </summary>
        bool TryValidate(string? token, out string userEmail, out string? moderationId);
    }
}
