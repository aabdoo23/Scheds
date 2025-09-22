using Microsoft.EntityFrameworkCore;
using Scheds.Domain.Configuration;
using Scheds.Infrastructure;
using Scheds.Infrastructure.Contexts;

namespace Scheds.MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddControllers();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromDays(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            builder.Services.AddDbContext<SchedsDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"));
            });

            // Configure admin settings
            builder.Services.Configure<AdminSettings>(builder.Configuration.GetSection("AdminSettings"));

            builder.Services.AddServices();
            builder.Services.AddRepositories();

            builder.Services.AddHttpClient();

            // Add authentication with Google
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "Google";
            })
            .AddCookie("Cookies", options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
            })
            .AddGoogle("Google", options =>
            {
                var google = builder.Configuration.GetSection("Authentication:Google");
                options.ClientId = google["ClientId"] ?? string.Empty;
                options.ClientSecret = google["ClientSecret"] ?? string.Empty;
                options.CallbackPath = google["CallbackPath"] ?? "/signin-google";
                options.Scope.Add("profile");
                options.Scope.Add("email");
            });

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

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
