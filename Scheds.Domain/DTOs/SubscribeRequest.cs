namespace Scheds.Domain.DTOs
{
    public class SubscribeRequest
    {
        public List<string> CourseSections { get; set; } = new List<string>();
    }
}