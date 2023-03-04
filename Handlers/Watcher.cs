using System.IO;
using System.Text.RegularExpressions;
using Watson.Abstractions;

namespace Watson.Handlers;

public class Watcher : IWatcher
{
    private readonly string _path;
    private readonly IActionProcessor _processor;
    private readonly bool _clean;

    public Watcher(IConfiguration configuration,
                   IActionProcessor processor)
    {
        if (configuration == null)
        {
            throw new ArgumentException("Application cannot start without configuration");
        }

        // set the path to the folder to monitor
        _path = configuration.GetValue<string>("ApplicationPaths:GoogleDrive"); // ?? Constants.Paths.Gdrive;
        _clean = configuration.GetValue<bool>("CleanOnInit");

        if (string.IsNullOrWhiteSpace(_path))
        {
            throw new ArgumentException("Application cannot start without a folder to watch");
        }

        Console.WriteLine($"Path: {_path}");
        _processor = processor;
    }

    public void Start()
    {
        Console.WriteLine($"Folder path: {_path}");
        Console.WriteLine($"Clean on init: {_clean}");

        ClearDirectory();

        // Create a new FileSystemWatcher object
        var watcher = new FileSystemWatcher()
        {
            Path = _path,
            Filter = "*.txt",
            NotifyFilter = NotifyFilters.LastWrite
                            | NotifyFilters.LastAccess
                            | NotifyFilters.FileName
                            | NotifyFilters.DirectoryName,
            EnableRaisingEvents = true
        };

        // Attach event handlers
        watcher.Created += OnCreated;
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
            return true;
        }

        var count = 0;
        var isDeleted = false;
        do
        {
            try
            {
                // delete file
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
                Console.WriteLine($"Application is not authorised to delete the file. Aborting...");
                return false;
            }
            finally
            {
                count++;
            }
        } while (!isDeleted && count < Constants.MaxRetries);

        return isDeleted;
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
            var fileContent = File.ReadAllText(filepath);
            var lines = File.ReadLines(filepath);

            // process each line as an action
            foreach (var line in lines)
            {
                Console.WriteLine($"Line: {line}");
                var parsed = Regex.Replace(line, @"\t|\r", "");
                _processor.ProcessLine(parsed);
            }

        }
        catch (IOException ex)
        {
            Console.WriteLine($"Could not read file: {filepath}. Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected exception while processing file: {filepath}. Error: {ex.Message}");
        } finally {
            // clear file
            File.Delete(filepath);
        }
    }
}