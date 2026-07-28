namespace Scheds.Domain.DTOs
{
    /// <summary>
    /// Envelope returned by schedule generation. Schedules are ranked best-first
    /// (lowest cost). Explored/Truncated let the UI be honest about how much of the
    /// combination space was searched before ranking.
    /// </summary>
    public class ScheduleGenerationResult
    {
        public List<List<ReturnedCardItemDTO>> Schedules { get; set; } = [];

        /// <summary>How many distinct valid schedules were scored.</summary>
        public int Explored { get; set; }

        /// <summary>True if a candidate/time cap stopped the search before the space was exhausted.</summary>
        public bool Truncated { get; set; }
    }
}
