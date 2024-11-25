using Microsoft.AspNetCore.Mvc;
using Scheds.Application.Interfaces.Repositories;
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
        public async Task<ActionResult<List<CardItem>>> GetCardByCourseCode(string courseCode)
        {
            var cardItems = await _repository.GetCardItemsByCourseCodeAsync(courseCode);
            if (cardItems == null)
            {
                return new NotFoundResult();
            }
            return cardItems;
        }

    }
}
