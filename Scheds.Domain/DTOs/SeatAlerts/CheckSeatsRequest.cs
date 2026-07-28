using System.ComponentModel.DataAnnotations;

namespace Scheds.Domain.DTOs.SeatAlerts
{
    /// <summary>
    /// Ad-hoc seat lookup for a set of course/section pairs.
    /// </summary>
    /// <remarks>
    /// The wire format is two parallel arrays for backwards compatibility. They are zipped once at
    /// the controller and a length mismatch is rejected, rather than being silently truncated as it
    /// was before.
    /// </remarks>
    public class CheckSeatsRequest
    {
        [Required]
        [MaxLength(SeatAlertLimits.MaxSectionsPerRequest)]
        public List<string> CourseCode { get; set; } = new();

        [Required]
        [MaxLength(SeatAlertLimits.MaxSectionsPerRequest)]
        public List<string> Sections { get; set; } = new();
    }
}
