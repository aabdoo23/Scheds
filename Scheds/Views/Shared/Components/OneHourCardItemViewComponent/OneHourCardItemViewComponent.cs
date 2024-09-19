using Microsoft.AspNetCore.Mvc;
using Scheds.Models;


namespace Scheds.Views.Shared.ViewComponents.CardItemViewComponent
{
    [ViewComponent(Name = "OneHourCardItemViewComponent")]
    public class OneHourCardItemViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(ReturnedCardItem model)
        {
            return View(model);
        }
    }
}
