using System.Diagnostics;
using Scheds.Domain.DTOs;
using Scheds.Domain.Entities;

namespace Scheds.Infrastructure.Util
{
    /// <summary>
    /// Collects the best schedules seen during enumeration without holding the whole
    /// combination space in memory: a bounded max-heap keeps the N lowest-cost
    /// schedules, a hash set dedupes, and four "axis leaders" guarantee the true #1
    /// for each client sort key (fewest days, least gaps, latest start, earliest
    /// finish) survives into the result even when the composite cost would drop it.
    /// A candidate cap and time budget bound worst-case work.
    /// </summary>
    public sealed class TopScheduleCollector
    {
        private sealed class Scored
        {
            public required List<CardItem> Items { get; init; }
            public required ScheduleMetrics Metrics { get; init; }
            public required double Cost { get; init; }
            public required string Hash { get; init; }
        }

        private const int SortKeyCount = 4; // 0 fewestDays, 1 leastGaps, 2 latestStart, 3 earliestFinish

        private readonly int _capacity;
        private readonly int _candidateLimit;
        private readonly long _timeBudgetMs;
        private readonly HashSet<string> _preferredInstructors;
        private readonly Stopwatch _sw;

        private readonly HashSet<string> _seen = [];
        // Max-heap on cost (worst schedule at the head) so the worst is evicted first.
        private readonly PriorityQueue<Scored, double> _heap =
            new(Comparer<double>.Create((x, y) => y.CompareTo(x)));
        private readonly Scored?[] _leaders = new Scored?[SortKeyCount];

        public int Explored { get; private set; }

        public TopScheduleCollector(int capacity, int candidateLimit, long timeBudgetMs,
            HashSet<string> preferredInstructors = null)
        {
            _capacity = Math.Max(0, capacity);
            _candidateLimit = candidateLimit;
            _timeBudgetMs = timeBudgetMs;
            _preferredInstructors = preferredInstructors ?? [];
            _sw = Stopwatch.StartNew();
        }

        /// <summary>Enumeration should stop: candidate cap reached or time budget spent.</summary>
        public bool ShouldStop =>
            Explored >= _candidateLimit || _sw.ElapsedMilliseconds >= _timeBudgetMs;

        /// <summary>
        /// Consider one completed, constraint-passing timetable. currentTimetable is
        /// snapshotted (it is mutated by backtracking after this returns).
        /// </summary>
        public void TryAdd(List<CardItem> currentTimetable, Dictionary<int, List<CardItem>> itemsPerDay)
        {
            string hash = HashOf(currentTimetable);
            if (!_seen.Add(hash)) return; // duplicate combination
            Explored++;

            var metrics = ScheduleScoring.ComputeMetrics(itemsPerDay);
            // Composite cost drives the heap + Recommended order. The preference bonus
            // only shifts this cost , the axis leaders below stay purely metric-based,
            // so the other client sort keys (days/gaps/start/finish) are unaffected.
            double cost = ScheduleScoring.Cost(metrics)
                - ScheduleScoring.PreferenceBonus(currentTimetable, _preferredInstructors);

            bool keepForHeap = false;
            if (_capacity > 0)
            {
                if (_heap.Count < _capacity) keepForHeap = true;
                else if (_heap.TryPeek(out _, out double worstCost)) keepForHeap = cost < worstCost;
            }
            bool keepForLeader = ImprovesAnyLeader(metrics, hash);

            if (!keepForHeap && !keepForLeader) return;

            var scored = new Scored
            {
                Items = new List<CardItem>(currentTimetable), // shallow snapshot; cards are immutable
                Metrics = metrics,
                Cost = cost,
                Hash = hash,
            };

            if (keepForHeap)
            {
                if (_heap.Count < _capacity) _heap.Enqueue(scored, cost);
                else _heap.DequeueEnqueue(scored, cost); // evict current worst
            }
            if (keepForLeader) UpdateLeaders(scored);
        }

        public ScheduleGenerationResult BuildResult()
        {
            var kept = new Dictionary<string, Scored>();
            foreach (var entry in _heap.UnorderedItems) kept[entry.Element.Hash] = entry.Element;
            foreach (var leader in _leaders)
                if (leader is not null) kept[leader.Hash] = leader;

            var ordered = kept.Values
                .OrderBy(s => s.Cost)
                .ThenBy(s => s.Hash, StringComparer.Ordinal)
                .Select(s => s.Items.Select(i => new ReturnedCardItemDTO(i)).ToList())
                .ToList();

            return new ScheduleGenerationResult
            {
                Schedules = ordered,
                Explored = Explored,
                Truncated = Explored >= _candidateLimit || _sw.ElapsedMilliseconds >= _timeBudgetMs,
            };
        }

        private bool ImprovesAnyLeader(in ScheduleMetrics m, string hash)
        {
            for (int k = 0; k < SortKeyCount; k++)
                if (_leaders[k] is null || IsBetterFor(k, m, hash, _leaders[k]!)) return true;
            return false;
        }

        private void UpdateLeaders(Scored s)
        {
            for (int k = 0; k < SortKeyCount; k++)
                if (_leaders[k] is null || IsBetterFor(k, s.Metrics, s.Hash, _leaders[k]!)) _leaders[k] = s;
        }

        // Mirrors the frontend rankOrder comparators so each leader == the client's #1.
        private static bool IsBetterFor(int key, in ScheduleMetrics a, string aHash, Scored cur)
        {
            var b = cur.Metrics;
            int cmp = key switch
            {
                0 => Cmp(a.DaysOnCampus, b.DaysOnCampus, a.GapMinutes, b.GapMinutes),        // fewest days
                1 => Cmp(a.GapMinutes, b.GapMinutes, a.DaysOnCampus, b.DaysOnCampus),        // least gaps
                2 => Cmp(b.EarliestStart, a.EarliestStart, a.DaysOnCampus, b.DaysOnCampus),  // latest start (max earliest)
                3 => Cmp(a.LatestEnd, b.LatestEnd, a.DaysOnCampus, b.DaysOnCampus),          // earliest finish
                _ => 0,
            };
            if (cmp != 0) return cmp < 0;
            return string.CompareOrdinal(aHash, cur.Hash) < 0; // deterministic tie-break
        }

        // Negative when (primaryA, secondaryA) beats (primaryB, secondaryB) on a
        // "smaller is better" basis.
        private static int Cmp(int primaryA, int primaryB, int secondaryA, int secondaryB)
        {
            if (primaryA != primaryB) return primaryA - primaryB;
            return secondaryA - secondaryB;
        }

        private static string HashOf(List<CardItem> items) =>
            string.Join("|", items.Select(i => $"{i.CourseCode}-{i.Section}").OrderBy(x => x, StringComparer.Ordinal));
    }
}
