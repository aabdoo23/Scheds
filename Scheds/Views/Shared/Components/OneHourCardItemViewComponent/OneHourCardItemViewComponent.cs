using Microsoft.AspNetCore.Mvc;
using Scheds.Domain.DTOs;

namespace Scheds.MVC.Views.Shared.Components.OneHourCardItemViewComponent
{
    [ViewComponent(Name = "OneHourCardItemViewComponent")]
    public class OneHourCardItemViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(ReturnedCardItemDTO model)
        {
            return View(model);
        }
    }
}
