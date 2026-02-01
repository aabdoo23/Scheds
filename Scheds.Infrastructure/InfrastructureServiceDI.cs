using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Scheds.Application.Interfaces.Repositories;
using Scheds.Application.Interfaces.Services;
using Scheds.Infrastructure.Repositories;
using Scheds.Infrastructure.Services;

namespace Scheds.Infrastructure
{
    public static class InfrastructureServiceDI
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<ICardItemRepository, CardItemRepository>();
            services.AddScoped<ICourseBaseRepository, CourseBaseRepository>();
            services.AddScoped<ICourseScheduleRepository, CourseScheduleRepository>();
            services.AddScoped<IInstructorRepository, InstructorRepository>();
            services.AddScoped<IScheduleGenerationRepository, ScheduleGenerationRepository>();
            services.AddScoped<ISeatModerationRepository, SeatModerationRepository>();
            services.AddScoped<ICartSeatModerationRepository, CartSeatModerationRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<ICardItemService, CardItemService>();
            services.AddScoped<ICourseBaseService, CourseBaseService>();
            services.AddScoped<IParsingService, ParsingService>();
            services.AddScoped<ISelfServiceLiveFetchService, SelfServiceLiveFetchService>();
            services.AddScoped<IEmptyRoomsService, EmptyRoomsService>();
            services.AddScoped<ISeatModerationService, SeatModerationService>();
            services.AddScoped<IEmailService, EmailService>();

            // Register background service for automated seat monitoring
            services.AddHostedService<SeatMonitoringBackgroundService>();

            return services;
        }

        public static AuthenticationBuilder AddCookieAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var useCrossOriginCookies = configuration.GetValue<bool>("UseCrossOriginCookies");
            return services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "Google";
            })
            .AddCookie("Cookies", options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
                options.Cookie.SameSite = useCrossOriginCookies ? SameSiteMode.None : SameSiteMode.Lax;
                options.Cookie.SecurePolicy = useCrossOriginCookies ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
            });
        }
    }
}
