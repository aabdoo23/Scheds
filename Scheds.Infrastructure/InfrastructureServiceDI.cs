﻿using Microsoft.Extensions.DependencyInjection;
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

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<ICardItemService, CardItemService>();
            services.AddScoped<IParsingService, ParsingService>();
            services.AddScoped<ISelfServiceLiveFetchService, SelfServiceLiveFetchService>();
            services.AddScoped<IEmptyRoomsService, EmptyRoomsService>();

            return services;
        }
    }
}
