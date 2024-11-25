using Microsoft.AspNetCore.Mvc;
using Scheds.Domain.DTOs;

namespace Scheds.MVC.Views.Shared.Components.ScheduleViewComponent
{
    [ViewComponent(Name = "ScheduleViewComponent")]
    public class ScheduleViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<ReturnedCardItemDTO> model)
        {
            return View(model);
        }
    }
}
