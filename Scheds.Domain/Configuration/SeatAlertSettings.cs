namespace Scheds.Domain.Configuration
{
    public class SeatAlertSettings
    {
        /// <summary>
        /// Absolute base URL that unsubscribe links point at. The monitor runs in a background
        /// service with no incoming request to infer a host from, so it has to be configured.
        /// Leave empty to send alerts without unsubscribe links.
        /// </summary>
        public string PublicBaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Optional dedicated key (base64) for signing unsubscribe tokens. Falls back to Jwt:Key.
        /// </summary>
        public string UnsubscribeKey { get; set; } = string.Empty;

        /// <summary>How long an unsubscribe link stays valid after the email is sent.</summary>
        public int UnsubscribeTokenDays { get; set; } = 60;

        /// <summary>
        /// How long stored seat data counts as fresh before the monitor refetches it. Separate
        /// from the 10-minute window used for general course browsing, because a seat alert that
        /// is ten minutes late is useless during registration.
        /// </summary>
        /// <remarks>
        /// Set this at or below <see cref="MonitorIntervalSeconds"/> for every cycle to see live
        /// data. Raising it cuts load on the university's Self-Service at the cost of latency.
        /// </remarks>
        public int SeatDataFreshnessSeconds { get; set; } = 45;

        /// <summary>Gap between monitoring cycles.</summary>
        public int MonitorIntervalSeconds { get; set; } = 60;

        /// <summary>
        /// How many courses to fetch from Self-Service at once. Bounded to stay polite to the
        /// university's server.
        /// </summary>
        public int MaxConcurrentFetches { get; set; } = 4;
    }
}
