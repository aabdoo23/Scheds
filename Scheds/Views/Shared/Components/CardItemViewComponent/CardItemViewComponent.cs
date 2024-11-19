using Microsoft.AspNetCore.Mvc;
using Scheds.Domain.DTOs;

namespace Scheds.MVC.Views.Shared.Components.CardItemViewComponent
{
    [ViewComponent(Name = "CardItemViewComponent")]
    public class CardItemViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(ReturnedCardItemDTO model)
        {
            return View(model);
        }
    }
}
