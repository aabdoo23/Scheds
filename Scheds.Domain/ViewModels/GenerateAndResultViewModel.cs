using Scheds.Domain.DTOs;

namespace Scheds.Domain.ViewModels
{
    public class GenerateAndResultViewModel
    {
        public GenerateRequestDTO FormRequest { get; set; }
        public List<List<ReturnedCardItemDTO>> ScheduleResult { get; set; }
    }
}
