namespace Scheds.Domain.ValueObjects
{
    /// <summary>
    /// A single course-section pair (e.g. CSCI101 section 01) — the unit that seat
    /// monitoring subscribes to and matches against live Self-Service data.
    /// </summary>
    /// <remarks>
    /// This type exists so that course/section pairing has exactly one canonical form.
    /// Previously the same pair was compared three different ways (exact string match in
    /// the cart, <c>PadLeft(2, '0')</c> in the monitor, and raw id equality on
    /// <see cref="Entities.SeatAlert"/>), and pairs were passed around as two
    /// parallel <c>List&lt;string&gt;</c> that callers could silently misalign.
    /// </remarks>
    public sealed class CourseSection : IEquatable<CourseSection>
    {
        /// <summary>Course code as stored and displayed, e.g. "CSCI101".</summary>
        public string CourseCode { get; }

        /// <summary>Section as stored and displayed, e.g. "01" or "01L".</summary>
        public string Section { get; }

        private CourseSection(string courseCode, string section)
        {
            CourseCode = courseCode;
            Section = section;
        }

        /// <summary>
        /// Comparison key for the section. Self-Service is inconsistent about zero-padding
        /// ("1" vs "01"), so both forms must resolve to the same section.
        /// </summary>
        private string SectionKey => Section.PadLeft(2, '0').ToUpperInvariant();

        /// <summary>
        /// Builds a pair, normalizing the course code to the upper-case trimmed form used
        /// everywhere else in the system. Returns null when either half is missing.
        /// </summary>
        public static CourseSection? Create(string? courseCode, string? section)
        {
            if (string.IsNullOrWhiteSpace(courseCode) || string.IsNullOrWhiteSpace(section))
                return null;

            return new CourseSection(courseCode.Trim().ToUpperInvariant(), section.Trim());
        }

        /// <summary>
        /// Parses a <see cref="Entities.SeatAlert"/> id of the form
        /// <c>{courseCode}_{section}</c>. Splits on the last separator so a course code
        /// that itself contains an underscore does not silently drop the subscription.
        /// </summary>
        public static CourseSection? TryParseId(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            var separator = id.LastIndexOf('_');
            if (separator <= 0 || separator == id.Length - 1)
                return null;

            return Create(id[..separator], id[(separator + 1)..]);
        }

        /// <summary>The <see cref="Entities.SeatAlert"/> primary key for this pair.</summary>
        public string ToId() => $"{CourseCode}_{Section}";

        public bool Equals(CourseSection? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return string.Equals(CourseCode, other.CourseCode, StringComparison.OrdinalIgnoreCase)
                && string.Equals(SectionKey, other.SectionKey, StringComparison.Ordinal);
        }

        public override bool Equals(object? obj) => Equals(obj as CourseSection);

        public override int GetHashCode() => HashCode.Combine(
            CourseCode.ToUpperInvariant(),
            SectionKey);

        public override string ToString() => $"{CourseCode} Section {Section}";
    }
}
