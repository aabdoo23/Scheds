namespace Scheds.Domain.DTOs
{
    public class CustomCourseBaseDTO
    {
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public List<string>? ExcludedMainSections { get; set; }
        public List<string>? ExcludedSubSections { get; set; }
        public List<string>? ExcludedProfessors { get; set; }
        public List<string>? ExcludedTAs { get; set; }

        public bool HasCustomSelection =>
            (ExcludedMainSections != null && ExcludedMainSections.Count > 0) ||
            (ExcludedSubSections != null && ExcludedSubSections.Count > 0) ||
            (ExcludedProfessors != null && ExcludedProfessors.Count > 0) ||
            (ExcludedTAs != null && ExcludedTAs.Count > 0);

        private static IEnumerable<string> FilterEmpty(IEnumerable<string>? source) =>
            source?.Where(s => !string.IsNullOrEmpty(s)) ?? [];

        public IEnumerable<string> EffectiveExcludedMainSections => FilterEmpty(ExcludedMainSections);
        public IEnumerable<string> EffectiveExcludedSubSections => FilterEmpty(ExcludedSubSections);
        public IEnumerable<string> EffectiveExcludedProfessors => FilterEmpty(ExcludedProfessors);
        public IEnumerable<string> EffectiveExcludedTAs => FilterEmpty(ExcludedTAs);
    }
}
