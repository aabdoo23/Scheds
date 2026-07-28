using Scheds.Domain.Entities;

namespace Scheds.Infrastructure.Util
{
    /// <summary>
    /// Quality metrics for a completed timetable and the cost used to rank schedules.
    /// Metric definitions (days, gap minutes, earliest/latest) mirror the frontend's
    /// scheduleMetrics exactly, so a client-side "least gaps" re-sort agrees with the
    /// server's ranking.
    /// </summary>
    public readonly struct ScheduleMetrics
    {
        public int DaysOnCampus { get; init; }
        public int GapMinutes { get; init; }
        public int EarliestStart { get; init; } // minutes from midnight
        public int LatestEnd { get; init; }     // minutes from midnight
        public int TotalSpanMinutes { get; init; }
    }

    public static class ScheduleScoring
    {
        // Cost weights (lower cost = better). Indicative , tune against real schedules.
        // One extra day on campus ≈ two hours of gaps; day span is a mild tie-breaker.
        public const double WeightDay = 120.0;
        public const double WeightGap = 1.0;
        public const double WeightSpan = 0.1;

        // Soft-preference reward: cost cut per course whose chosen instructor is one the
        // student preferred. ~0.85 of a day , strong enough to reorder, not to override
        // compactness wholesale. Applies once per course (a preferred prof teaching both
        // a lecture and its lab is not double-counted).
        public const double PreferredProfessorBonus = 100.0;

        public static ScheduleMetrics ComputeMetrics(Dictionary<int, List<CardItem>> itemsPerDay)
        {
            int days = 0, gap = 0, span = 0;
            int earliest = int.MaxValue, latest = int.MinValue;

            foreach (var kv in itemsPerDay)
            {
                var items = kv.Value;
                if (items.Count == 0) continue;
                days++;

                var sorted = items.OrderBy(i => i.GetStartTime()).ToList();
                int dayEarliest = (int)sorted[0].GetStartTime().TotalMinutes;
                int dayLatest = int.MinValue;

                for (int i = 0; i < sorted.Count; i++)
                {
                    int s = (int)sorted[i].GetStartTime().TotalMinutes;
                    int e = (int)sorted[i].GetEndTime().TotalMinutes;

                    if (s < earliest) earliest = s;
                    if (e > latest) latest = e;
                    if (e > dayLatest) dayLatest = e;

                    if (i > 0)
                    {
                        int prevEnd = (int)sorted[i - 1].GetEndTime().TotalMinutes;
                        if (s > prevEnd) gap += s - prevEnd;
                    }
                }

                span += dayLatest - dayEarliest;
            }

            if (days == 0)
            {
                earliest = 0;
                latest = 0;
            }

            return new ScheduleMetrics
            {
                DaysOnCampus = days,
                GapMinutes = gap,
                EarliestStart = earliest,
                LatestEnd = latest,
                TotalSpanMinutes = span,
            };
        }

        public static double Cost(in ScheduleMetrics m) =>
            WeightDay * m.DaysOnCampus + WeightGap * m.GapMinutes + WeightSpan * m.TotalSpanMinutes;

        /// <summary>
        /// Total cost reduction for a schedule from satisfied professor preferences:
        /// one bonus per distinct course whose instructor is in the preferred set.
        /// </summary>
        public static double PreferenceBonus(IEnumerable<CardItem> items, HashSet<string> preferredInstructors)
        {
            if (preferredInstructors == null || preferredInstructors.Count == 0) return 0;

            var satisfiedCourses = new HashSet<string>();
            foreach (var it in items)
            {
                if (!string.IsNullOrEmpty(it.Instructor) && preferredInstructors.Contains(it.Instructor))
                {
                    satisfiedCourses.Add(it.CourseCode);
                }
            }
            return satisfiedCourses.Count * PreferredProfessorBonus;
        }
    }
}
