using Watson.Abstractions;

namespace Watson;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IWatcher _watcher;
    private readonly bool _clean;
    private readonly string? _path;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    public Worker(ILogger<Worker> logger,
                  IConfiguration configuration,
                  IWatcher watcher,
                  IHostApplicationLifetime hostApplicationLifetime
                  )
    {

        _logger = logger;
        _watcher = watcher;

        _clean = configuration.GetValue<bool>(Constants.Configuration.CleanOnInit);

        // set the path to the folder to monitor
        _path = configuration.GetValue<string>(Constants.Configuration.WatchedFolder);
        _path = string.IsNullOrEmpty(_path) ? _path : Environment.ExpandEnvironmentVariables(_path);

        Console.WriteLine($"Watched path: {_path}");
        Console.WriteLine($"Clean on init: {_clean}");

        //TODO: not sure if needed...
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ClearDirectory();

        // check if multiple instances are running

        _watcher.Start();
    }

    private void ClearDirectory()
    {
        if (!_clean || !Directory.Exists(_path))
        {
            return;
        }

        foreach (string file in Directory.GetFiles(_path, Constants.Filter))
        {
            Console.WriteLine("Clearing file...");

            var result = RetryDelete(file);

            if (!result)
            {
                Console.WriteLine($"Failed to delete file {file}");
            }
        }
    }

    private bool RetryDelete(string file)
    {
        if (!File.Exists(file))
        {
            Console.WriteLine("File does not exist... Skipping.");
            return true;
        }

        var count = 0;
        var isDeleted = false;
        do
        {
            try
            {
                Console.WriteLine($"Deleting file... {file}");
                File.Delete(file);

                isDeleted = true;
            }
            catch (IOException)
            {
                Console.WriteLine($"File is in use. Retrying in {Constants.RetryDelay} ms...");
                Thread.Sleep(Constants.RetryDelay);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Application is not authorised to delete the file. Aborting...");
                return false;
            }
            finally
            {
                count++;
            }
        } while (!isDeleted && count < Constants.MaxRetries);

        return isDeleted;
    }
}
