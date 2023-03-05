using Watson.Abstractions;

namespace Watson.Handlers;

public class Watcher : IWatcher
{
    private readonly string? _path;
    private readonly string? _archive;

    private readonly IActionProcessor _processor;

    public Watcher(IConfiguration configuration,
                   IActionProcessor processor)
    {
        if (configuration == null)
        {
            throw new ArgumentException("Application cannot start without configuration");
        }

        _processor = processor;

        // set the path to the folder to monitor
        _path = configuration.GetValue<string>(Constants.Configuration.WatchedFolder);
        _path = string.IsNullOrEmpty(_path) ? _path : Environment.ExpandEnvironmentVariables(_path);

        Console.WriteLine($"Watched path: {_path}");

        if (!Directory.Exists(_path))
        {
            throw new ArgumentException("Application cannot start without a folder to watch");
        }

        _archive = Path.Combine(_path, "archive");
    }

    public void Start()
    {
        if (_path == null)
        {
            return;
        }

        var watcher = new FileSystemWatcher
        {
            Path = _path,
            Filter = Constants.Filter,
            NotifyFilter = NotifyFilters.CreationTime
                         | NotifyFilters.FileName
                         | NotifyFilters.DirectoryName,
            EnableRaisingEvents = true
        };

        watcher.Created += OnCreated;

        watcher.Changed += DoNothing;
        watcher.Deleted += DoNothing;
        watcher.Renamed += DoNothing;
    }

    // define event handlers for the file change events
    private void DoNothing(object sender, FileSystemEventArgs e) {}
    
    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        var filepath = e.FullPath;

        if (!File.Exists(filepath))
        {
            Console.WriteLine("File doesn't exist.");
            return;
        }

        Console.WriteLine($"File created: {filepath}");

        try
        {
            _processor.Process(filepath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected exception while processing file: {filepath}. Error: {ex.Message}. Trace: {ex.StackTrace}");
        }
        finally
        {
            Thread.Sleep(500);
            File.Delete(filepath);
            // Archive(filepath, e.Name);
            Thread.Sleep(500);
        }
    }

    private void Archive(string filepath, string? filename)
    {
        if (string.IsNullOrWhiteSpace(_archive) || string.IsNullOrWhiteSpace(filename))
        {
            Console.WriteLine("Unable to archive without a filename or destination folder");
            return;
        }

        if (!Directory.Exists(_archive))
        {
            Directory.CreateDirectory(_archive);
        }

        File.Move(filepath, Path.Combine(_archive, filename), true);
    }
}