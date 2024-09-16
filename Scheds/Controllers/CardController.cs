using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Scheds.DAL.Repositories;
using Scheds.DAL.Services;
using Scheds.Models;
using System.Text;

namespace Scheds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CardController
    {
        private readonly CardItemRepository repository;
        private readonly ParsingService parsingService;
        public CardController(CardItemRepository repository, ParsingService parsingService)
        {
            this.repository = repository;
            this.parsingService = parsingService;
        }
        [HttpGet]
        public async Task<ActionResult<List<CardItem>>> GetAllCards()
        {
            var cardItems = await repository.GetAllCardItemsAsync();
            return cardItems;
        }
        [HttpGet("byId/{id}")]
        public async Task<ActionResult<CardItem>> GetCard(int id)
        {
            var cardItem = await repository.GetCardItemByIdAsync(id);
            if (cardItem == null)
            {
                return new NotFoundResult();
            }
            return cardItem;
        }

        [HttpGet("{courseCode}")]
        public async Task<ActionResult<List<CardItem>>> GetCardByCourseCode(string courseCode)
        {
            var cardItems = await repository.GetCardItemsByCourseCodeAsync(courseCode);
            if (cardItems == null)
            {
                return new NotFoundResult();
            }
            return cardItems;
        }
        
    }
}
