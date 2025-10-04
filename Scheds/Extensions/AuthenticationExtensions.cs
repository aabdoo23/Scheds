using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;

namespace Scheds.MVC.Extensions
{
    public static class AuthenticationExtensions
    {
        public static AuthenticationBuilder AddGoogleAuthentication(this AuthenticationBuilder builder, IConfiguration configuration)
        {
            builder.AddGoogle("Google", options =>
            {
                var google = configuration.GetSection("Authentication:Google");
                options.ClientId = google["ClientId"] ?? string.Empty;
                options.ClientSecret = google["ClientSecret"] ?? string.Empty;
                options.CallbackPath = google["CallbackPath"] ?? "/signin-google";
                options.Scope.Add("profile");
                options.Scope.Add("email");
            });

            return builder;
        }
    }
}