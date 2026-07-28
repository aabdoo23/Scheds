namespace Scheds.Domain.Configuration
{
    /// <summary>Named rate limit policies, so the controller and startup cannot disagree.</summary>
    public static class RateLimitPolicies
    {
        /// <summary>Cheap database-only reads.</summary>
        public const string SeatAlertsRead = "seat-alerts-read";

        /// <summary>Reads that can trigger a fetch from the university's Self-Service.</summary>
        public const string SeatAlertsLive = "seat-alerts-live";

        /// <summary>Subscribe and unsubscribe.</summary>
        public const string SeatAlertsWrite = "seat-alerts-write";

        /// <summary>Token-authenticated unsubscribe pages, keyed by IP rather than user.</summary>
        public const string SeatAlertsUnsubscribe = "seat-alerts-unsubscribe";
    }
}
