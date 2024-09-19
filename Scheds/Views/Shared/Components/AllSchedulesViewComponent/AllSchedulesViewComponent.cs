using Microsoft.AspNetCore.Mvc;
using Scheds.Models;


namespace Scheds.Views.Shared.ViewComponents.AllSchedulesViewComponent
{
    [ViewComponent(Name = "AllSchedulesViewComponent")]
    public class AllSchedulesViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<List<ReturnedCardItem>> model)
        {
            return View(model);
        }
    }
}
