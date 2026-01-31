namespace Scheds.Domain.DTOs.Admin
{
    public class GenerationListItemDTO
    {
        public int Id { get; set; }
        public DateTime GeneratedAt { get; set; }
        public int NumberOfSchedulesGenerated { get; set; }
        public int SelectedCoursesCount { get; set; }
        public int SelectedCustomCoursesCount { get; set; }
        public bool UsedLiveData { get; set; }
        public bool IsEngineering { get; set; }
    }
}
