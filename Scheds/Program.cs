using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Scheds.Domain.Configuration;
using Scheds.Infrastructure;
using Scheds.Infrastructure.Contexts;
using System.Threading.RateLimiting;

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
            builder.Services.AddMemoryCache();
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // Partition by signed-in student so one person cannot exhaust everyone's budget,
                // falling back to remote IP for the anonymous unsubscribe pages.
                static string PartitionKey(HttpContext context) =>
                    context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                    ?? context.Connection.RemoteIpAddress?.ToString()
                    ?? "unknown";

                void AddFixedWindow(string policy, int permitLimit) =>
                    options.AddPolicy(policy, context =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            $"{policy}:{PartitionKey(context)}",
                            _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = permitLimit,
                                Window = TimeSpan.FromMinutes(1)
                            }));

                AddFixedWindow(RateLimitPolicies.SeatAlertsRead, 60);
                // Tightest: each of these can fan out into fetches against the university's portal.
                AddFixedWindow(RateLimitPolicies.SeatAlertsLive, 12);
                AddFixedWindow(RateLimitPolicies.SeatAlertsWrite, 30);
                AddFixedWindow(RateLimitPolicies.SeatAlertsUnsubscribe, 20);
            });
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
            builder.Services.Configure<SeatAlertSettings>(builder.Configuration.GetSection("SeatAlerts"));

            builder.Services.AddServices();
            builder.Services.AddRepositories();

            builder.Services.AddHttpClient();
            builder.Services.AddHttpClient("SelfServiceApi", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(60);
                client.DefaultRequestHeaders.Add("Origin", "https://register.nu.edu.eg");
                client.DefaultRequestHeaders.Add("Referer", "https://register.nu.edu.eg/PowerCampusSelfService/Registration/Courses");
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer(),
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            });

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
            // Between authentication and authorization on purpose: the principal is already
            // resolved, so limits partition per student, while an unauthenticated flood is still
            // capped instead of being waved through to a free 401 each time.
            app.UseRateLimiter();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
