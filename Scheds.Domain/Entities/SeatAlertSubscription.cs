namespace Scheds.Domain.Entities
{
    /// <summary>
    /// One user watching one section.
    /// </summary>
    /// <remarks>
    /// This was an implicit many-to-many join. It is explicit now because notification state
    /// has to be per user: with a single shared timestamp on <see cref="SeatAlert"/>, a
    /// student who subscribed just after someone else was alerted inherited that cooldown and
    /// waited up to 15 minutes while seats were open.
    ///
    /// Mapped to the original "UserSeatAlerts" table so existing subscriptions survive.
    /// </remarks>
    public class SeatAlertSubscription
    {
        public string SeatAlertId { get; set; } = string.Empty;
        public virtual SeatAlert SeatAlert { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// When this user was last emailed about this section. Null means never — which is what
        /// makes a brand new subscriber eligible immediately, even mid-opening.
        /// </summary>
        public DateTime? LastNotifiedAt { get; set; }

        /// <summary>
        /// Whether this user should be alerted about the opening that began at
        /// <paramref name="openSince"/>.
        /// </summary>
        /// <param name="reNotifyFloor">
        /// Minimum gap between two alerts for this user and section. Alerts are edge-triggered, so
        /// this is not a general cooldown — it only guards against Self-Service flapping a section
        /// open and shut repeatedly.
        /// </param>
        public bool ShouldNotify(DateTime? openSince, DateTime now, TimeSpan reNotifyFloor)
        {
            if (openSince == null)
                return false;

            // Never told this user about this section — includes someone who just subscribed to a
            // section that is already open.
            if (LastNotifiedAt == null)
                return true;

            // Already told them about this opening.
            if (LastNotifiedAt >= openSince)
                return false;

            // A new opening, but throttle a section that is flapping open and shut.
            return now - LastNotifiedAt.Value >= reNotifyFloor;
        }
    }
}
