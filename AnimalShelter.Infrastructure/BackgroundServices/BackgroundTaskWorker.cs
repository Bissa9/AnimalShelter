using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AnimalShelter.Infrastructure.BackgroundServices;

public class BackgroundTaskWorker : BackgroundService
{
    private readonly IBackgroundTaskQueue _queue;
    private readonly ILogger<BackgroundTaskWorker> _logger;

    public BackgroundTaskWorker(
        IBackgroundTaskQueue queue,
        ILogger<BackgroundTaskWorker> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Background task worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItem =
                    await _queue.DequeueAsync(stoppingToken);

                await workItem(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while processing background task.");
            }
        }

        _logger.LogInformation(
            "Background task worker stopped.");
    }
}