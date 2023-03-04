using Watson.Abstractions;

namespace Watson;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IWatcher _watcher;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    public Worker(ILogger<Worker> logger,
                  IWatcher watcher,
                  IHostApplicationLifetime hostApplicationLifetime
                  )
    {

        _logger = logger;
        _watcher = watcher;

        //TODO: not sure if needed...
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // check if multiple instances are running

        _watcher.Start();

        // force stop
        //_hostApplicationLifetime.StopApplication();
        // while (!stoppingToken.IsCancellationRequested)
        // {
        //     _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        // }
    }
}
