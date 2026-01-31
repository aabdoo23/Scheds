using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Scheds.MVC.Filters
{
    public class AdminAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private const string AdminSessionKey = "AdminAuthenticated";

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.Session.GetString(AdminSessionKey) != "true")
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
