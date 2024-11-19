using Microsoft.AspNetCore.Mvc;
using Scheds.Domain.DTOs;


namespace Scheds.MVC.Views.Shared.Components.AllSchedulesViewComponent
{
    [ViewComponent(Name = "AllSchedulesViewComponent")]
    public class AllSchedulesViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<List<ReturnedCardItemDTO>> model)
        {
            return View(model);
        }
    }
}
