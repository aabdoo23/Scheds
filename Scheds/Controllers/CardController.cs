using Microsoft.AspNetCore.Mvc;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Domain.DTOs;
using Scheds.Domain.Entities;

namespace Scheds.MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CardController(ICardItemRepository repository)
    {
        private readonly ICardItemRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

        [HttpGet]
        public async Task<ActionResult<List<CardItem>>> GetAllCards()
        {
            var cardItems = await _repository.GetAllAsync();
            return cardItems.ToList();
        }

        [HttpGet("byId/{id}")]
        public async Task<ActionResult<CardItem>> GetCard(string id)
        {
            var cardItem = await _repository.GetByIdAsync(id);
            if (cardItem == null)
            {
                return new NotFoundResult();
            }
            return cardItem;
        }

        [HttpGet("{courseCode}")]
        public async Task<ActionResult<List<CardItemSummaryDTO>>> GetCardByCourseCode(string courseCode)
        {
            var cardItems = await _repository.GetCardItemsByCourseCodeAsync(courseCode);
            if (cardItems == null)
            {
                return new NotFoundResult();
            }
            var summaries = cardItems.Select(c =>
            {
                var scheduleDisplay = c.CourseSchedules.Count > 0
                    ? string.Join(", ", c.CourseSchedules.Select(s =>
                        $"{s.DayOfWeek[..Math.Min(3, s.DayOfWeek.Length)]} {s.StartTime.ToString(@"hh\:mm")}-{s.EndTime.ToString(@"hh\:mm")}"))
                    : null;
                return new CardItemSummaryDTO
                {
                    CourseCode = c.CourseCode,
                    Instructor = c.Instructor,
                    Section = c.Section,
                    SubType = c.SubType,
                    ScheduleDisplay = scheduleDisplay
                };
            }).ToList();
            return summaries;
        }
    }
}
