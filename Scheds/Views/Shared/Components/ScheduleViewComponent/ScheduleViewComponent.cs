using Microsoft.AspNetCore.Mvc;
using Scheds.Models;


namespace Scheds.Views.Shared.ViewComponents.ScheduleViewComponent
{
    [ViewComponent(Name = "ScheduleViewComponent")]
    public class ScheduleViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<List<ReturnedCardItem>> model)
        {
            return View(model);
        }
    }
}
