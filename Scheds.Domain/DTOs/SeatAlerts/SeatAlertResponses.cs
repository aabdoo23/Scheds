namespace Scheds.Domain.DTOs.SeatAlerts
{
    /// <summary>
    /// Typed seat alert responses. These replace anonymous objects whose casing the frontend could
    /// only guess at — it carried a PascalCase/camelCase fallback on every field as a result.
    /// </summary>
    public class SeatAlertResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? Message { get; set; }

        public static SeatAlertResult Ok(string? message = null) =>
            new() { Success = true, Message = message };

        public static SeatAlertResult Failed(string error) =>
            new() { Success = false, Error = error };
    }

    /// <summary>A watched section without seat data.</summary>
    public class WatchedSectionDto
    {
        public string CourseCode { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
    }

    public class WatchedSectionsResponse
    {
        public bool Success { get; set; } = true;
        public List<WatchedSectionDto> Sections { get; set; } = new();
    }

    /// <summary>A section with its current seat reading.</summary>
    public class SeatReadingDto
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string Instructor { get; set; } = string.Empty;
        public bool HasSeats { get; set; }
        public int SeatsLeft { get; set; }

        /// <summary>When this reading was taken, as ISO-8601 UTC.</summary>
        public string LastUpdate { get; set; } = string.Empty;
    }

    public class SeatReadingsResponse
    {
        public bool Success { get; set; } = true;

        /// <summary>When the server produced this response, as ISO-8601 UTC.</summary>
        public string CheckedAt { get; set; } = string.Empty;

        public List<SeatReadingDto> Sections { get; set; } = new();
    }
}
