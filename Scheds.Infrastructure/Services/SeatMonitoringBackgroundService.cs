
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scheds.Application.Interfaces.Services;
using Scheds.Domain.Configuration;

namespace Scheds.Infrastructure.Services
{
    public class SeatMonitoringBackgroundService(
        ILogger<SeatMonitoringBackgroundService> logger,
        IServiceProvider serviceProvider,
        IOptions<SeatAlertSettings> seatAlertSettings) : BackgroundService
    {
        private readonly ILogger<SeatMonitoringBackgroundService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        private readonly SeatAlertSettings _seatAlertSettings = seatAlertSettings?.Value ?? throw new ArgumentNullException(nameof(seatAlertSettings));

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = TimeSpan.FromSeconds(Math.Max(15, _seatAlertSettings.MonitorIntervalSeconds));
            _logger.LogInformation("Seat monitoring background service started, cycle every {Interval}", interval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();

                    var seatAlertService = scope.ServiceProvider.GetRequiredService<ISeatAlertService>();

                    await seatAlertService.MonitorAllCourses(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during seat monitoring background service");
                }

                try
                {
                    // Delay after the work, so a cycle slower than the interval cannot overlap
                    // itself; cycles drift instead of piling up.
                    await Task.Delay(interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("Seat monitoring background service stopped");
        }
    }
}
