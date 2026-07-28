using System.ComponentModel.DataAnnotations;

namespace Scheds.Domain.DTOs.SeatAlerts
{
    /// <summary>
    /// One section to start or stop watching.
    /// </summary>
    /// <remarks>
    /// These fields were previously unvalidated, and they become the seat alert's primary key
    /// (<c>{courseCode}_{section}</c>) and are interpolated into alert emails. Bounding them keeps
    /// junk out of the database, out of the monitoring cycle, and out of the index on
    /// <c>CardItems.CourseCode</c>.
    /// </remarks>
    public class WatchSectionRequest
    {
        [Required]
        [RegularExpression(SeatAlertLimits.CourseCodePattern,
            ErrorMessage = "courseCode must be 2-20 letters, digits or dashes")]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        [RegularExpression(SeatAlertLimits.SectionPattern,
            ErrorMessage = "section must be 1-10 letters or digits")]
        public string Section { get; set; } = string.Empty;
    }
}
