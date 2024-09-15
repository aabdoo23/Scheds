namespace Scheds.Models
{
    public class GenerateAndResultViewModel
    {
        public GenerateRequest FormRequest { get; set; }
        public List<List<List<ReturnedCardItem>>> ScheduleResult { get; set; }
    }
}
