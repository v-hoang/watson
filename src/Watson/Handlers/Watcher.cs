using System.Text.RegularExpressions;
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
            NotifyFilter = NotifyFilters.LastWrite
                        | NotifyFilters.LastAccess
                        | NotifyFilters.FileName
                        | NotifyFilters.DirectoryName,
            EnableRaisingEvents = true
        };

        // watcher.Changed += OnCreated;
        watcher.Created += OnCreated;
    }

    // define event handlers for the file change events
    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        var filepath = e.FullPath;
        Console.WriteLine($"File created: {filepath}");

        if (!File.Exists(filepath))
        {
            Console.WriteLine("File doesn't exist.");
            return;
        }

        try
        {
            // read all lines
            var lines = File.ReadLines(filepath);

            // process each line as an action
            foreach (var line in lines)
            {
                Console.WriteLine($"Line: {line}");
                var parsed = Regex.Replace(line, @"\t|\r", "");
                _processor.ProcessLine(parsed);
            }

            Console.WriteLine("Finished processing file");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Could not read file: {filepath}. Error: {ex.Message}. Trace: {ex.StackTrace}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected exception while processing file: {filepath}. Error: {ex.Message}. Trace: {ex.StackTrace}");
        }
        finally
        {
            File.Delete(filepath);
            // Delete(filepath);
            // Archive(filepath, e.Name);
        }
    }

    private void Delete(string filepath)
    {
        if (!File.Exists(filepath))
        {
            return;
        }

        // ... hidding before deleting...
        File.SetAttributes(filepath, FileAttributes.Hidden);

        // while (FileInUse(filepath)) ;
        File.Delete(filepath);
        // while (File.Exists(filepath)) ;
    }

    public bool FileInUse(string file)
    {
        if (File.Exists(file))
        {
            try
            {
                using (Stream stream = new FileStream(file, FileMode.Open))
                {
                    // File/Stream manipulating code here
                    return false;
                }
            }
            catch
            {
                Thread.Sleep(50);
                return true;
            }
        }
        return false;
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