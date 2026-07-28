namespace Scheds.Domain.Entities
{
    /// <summary>
    /// Whether a course/section pair is known to exist.
    /// </summary>
    public enum SectionExistence
    {
        /// <summary>
        /// No stored data for the course, so nothing can be concluded. Callers should let the
        /// request through — rejecting here would block legitimate sections whenever the cache is
        /// cold.
        /// </summary>
        Unknown,

        /// <summary>The section is present in stored data for its course.</summary>
        Exists,

        /// <summary>
        /// Sections are stored for this course and this is not one of them, so it is provably wrong.
        /// </summary>
        NotFound
    }
}
