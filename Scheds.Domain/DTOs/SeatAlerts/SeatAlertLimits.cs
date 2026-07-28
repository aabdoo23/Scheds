namespace Scheds.Domain.DTOs.SeatAlerts
{
    /// <summary>Bounds on seat alert input, shared by the request DTOs.</summary>
    public static class SeatAlertLimits
    {
        public const string CourseCodePattern = @"^[A-Za-z0-9\-]{2,20}$";
        public const string SectionPattern = @"^[A-Za-z0-9]{1,10}$";

        /// <summary>
        /// Most course/section pairs one request may ask about. Each distinct course can cost an
        /// upstream fetch, so an unbounded list is a scrape amplifier.
        /// </summary>
        public const int MaxSectionsPerRequest = 40;
    }
}
