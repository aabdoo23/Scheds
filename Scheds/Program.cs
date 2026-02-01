using Microsoft.EntityFrameworkCore;
using Scheds.Domain.Configuration;
using Scheds.Infrastructure;
using Scheds.Infrastructure.Contexts;
using Scheds.MVC.Extensions;

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
            builder.Services.Configure<FrontendSettings>(builder.Configuration.GetSection("FrontendSettings"));

            builder.Services.AddServices();
            builder.Services.AddRepositories();

            builder.Services.AddHttpClient();

            var frontendUrl = builder.Configuration["FrontendSettings:Url"]?.TrimEnd('/');
            if (!string.IsNullOrEmpty(frontendUrl))
            {
                builder.Services.AddCors(options =>
                {
                    options.AddDefaultPolicy(policy =>
                    {
                        policy.WithOrigins(frontendUrl)
                            .AllowCredentials()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
                });
            }

            // Add authentication
            builder.Services.AddCookieAuthentication()
                .AddGoogleAuthentication(builder.Configuration);

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
            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
