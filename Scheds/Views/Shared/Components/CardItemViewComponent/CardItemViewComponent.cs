using Microsoft.AspNetCore.Mvc;
using Scheds.Models;


namespace Scheds.Views.Shared.ViewComponents.CardItemViewComponent
{
    [ViewComponent(Name = "CardItemViewComponent")]
    public class CardItemViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(ReturnedCardItem model)
        {
            return View(model);
        }
    }
}
