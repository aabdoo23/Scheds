namespace Scheds.Domain.DTOs
{
    public class SubscribeRequestDTO
    {
        public List<string> CourseSections { get; set; } = new List<string>();
    }
}