
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scheds.Application.Interfaces.Services;

namespace Scheds.Infrastructure.Services
{
    public class SeatMonitoringBackgroundService(ILogger<SeatMonitoringBackgroundService> logger, IServiceProvider serviceProvider) : BackgroundService
    {
        private readonly ILogger<SeatMonitoringBackgroundService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Seat monitoring background service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Seat monitoring check started at: {time}", DateTimeOffset.Now);
                    
                    using var scope = _serviceProvider.CreateScope();
                    
                    var seatModerationService = scope.ServiceProvider.GetRequiredService<ISeatModerationService>();
                    
                    await seatModerationService.MonitorAllCourses(stoppingToken);
                    
                    _logger.LogInformation("Seat monitoring check completed at: {time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during seat monitoring background service");
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

    }
}