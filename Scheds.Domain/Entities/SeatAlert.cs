using System.Collections.Generic;
using Scheds.Domain.Entities.Common;

namespace Scheds.Domain.Entities
{
    /// <summary>
    /// A watched course section. <see cref="BaseEntity.Id"/> is the
    /// <c>{courseCode}_{section}</c> pair — see <c>CourseSection.ToId()</c>.
    /// </summary>
    public class SeatAlert : BaseEntity
    {
        public virtual ICollection<SeatAlertSubscription> Subscriptions { get; set; }
            = new List<SeatAlertSubscription>();

        /// <summary>Last time the monitor observed this section's seat count.</summary>
        public DateTime LastCheckedAt { get; set; }

        /// <summary>
        /// Seats observed on the previous cycle. Null means never observed, which counts as
        /// "was full" so an already-open section still alerts its first watcher.
        /// </summary>
        public int? LastSeenSeats { get; set; }

        /// <summary>
        /// When the current opening began, or null while the section is full. Alerts are
        /// triggered by this moving forward, not by seats merely being available, so a section
        /// that stays open does not re-email everyone every cycle.
        /// </summary>
        public DateTime? OpenSince { get; set; }

        public SeatAlert()
        {
        }

        public SeatAlert(string courseCodeSection)
        {
            Id = courseCodeSection;
        }

        /// <summary>
        /// Records one cycle's seat count and maintains <see cref="OpenSince"/>. Returns true when
        /// the section just went from full to open.
        /// </summary>
        public bool RecordObservation(int seatsLeft, DateTime observedAt)
        {
            // A null LastSeenSeats (never observed) counts as full, so an already-open section
            // still starts an opening window for its first watcher.
            var wasOpen = LastSeenSeats > 0;
            var isOpen = seatsLeft > 0;
            var openedNow = isOpen && !wasOpen;

            if (!isOpen)
                OpenSince = null;
            else if (openedNow || OpenSince == null)
                OpenSince = observedAt;

            LastSeenSeats = seatsLeft;
            LastCheckedAt = observedAt;
            return openedNow;
        }
    }
}
