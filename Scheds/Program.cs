using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scheds.DAL;
using Scheds.DAL.Repositories;
using Scheds.DAL.Services;
using Scheds.Models.Forum;

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
            builder.Services.AddScoped<UserRepository>();

            //builder.Services.AddIdentity<User, IdentityRole>()
            //    .AddEntityFrameworkStores<SchedsDbContext>()
            //    .AddDefaultTokenProviders();
            //builder.Services.Configure<IdentityOptions>(options =>
            //{
            //    options.Password.RequiredLength = 6;
            //    options.Password.RequireDigit = true;
            //});


            builder.Services.AddHttpClient();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseRouting();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
