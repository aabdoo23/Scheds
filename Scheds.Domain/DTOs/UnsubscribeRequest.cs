namespace Scheds.Domain.DTOs
{
    public class UnsubscribeRequest
    {
        public List<string> CourseSections { get; set; } = new List<string>();
    }
}