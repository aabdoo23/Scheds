using Microsoft.EntityFrameworkCore;
using Scheds.DAL;
using Scheds.DAL.Repositories;
using Scheds.DAL.Services;

namespace Scheds
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddControllers();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromDays(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            builder.Services.AddDbContext<SchedsDbContext>(options => {
                options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"));
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            });
            builder.Services.AddScoped<CardItemRepository>();
            builder.Services.AddScoped<AllInstructorsRepository>();
            builder.Services.AddScoped<CourseBaseRepository>();
            builder.Services.AddScoped<CourseScheduleRepository>();
            builder.Services.AddScoped<ParsingService>();
            builder.Services.AddScoped<NuDealer>();


            builder.Services.AddHttpClient();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
