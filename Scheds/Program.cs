using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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
            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Path.GetTempPath(), "scheds-dataprotection-keys")));
            var useCrossOriginCookies = builder.Configuration.GetValue<bool>("UseCrossOriginCookies");
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromDays(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = useCrossOriginCookies ? SameSiteMode.None : SameSiteMode.Lax;
                options.Cookie.SecurePolicy = useCrossOriginCookies ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
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

            var authBuilder = builder.Services.AddAuthentication(options => options.DefaultScheme = "Bearer");
            var jwtKey = builder.Configuration["Jwt:Key"];
            if (!string.IsNullOrEmpty(jwtKey))
            {
                var keyBytes = Convert.FromBase64String(jwtKey);
                var issuer = builder.Configuration["Jwt:Issuer"] ?? "scheds";
                var audience = builder.Configuration["Jwt:Audience"] ?? "scheds-frontend";
                authBuilder.AddJwtBearer("Bearer", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };
                });
            }

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        var path = context.Request.Path.Value ?? "";
                        if (path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
                        {
                            var feature = context.Features.Get<IExceptionHandlerPathFeature>();
                            var ex = feature?.Error;
                            var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
                            var config = context.RequestServices.GetRequiredService<IConfiguration>();
                            var exposeErrors = config.GetValue<bool>("ExposeApiErrors");
                            var msg = (env.IsDevelopment() || exposeErrors) && ex != null ? ex.Message : "An error occurred";
                            if (frontendUrl != null)
                            {
                                context.Response.Headers.Append("Access-Control-Allow-Origin", frontendUrl);
                                context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
                            }
                            context.Response.StatusCode = 500;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsJsonAsync(new { error = msg, requestId = context.TraceIdentifier });
                        }
                        else
                        {
                            context.Response.Redirect("/Home/Error");
                        }
                    });
                });
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
