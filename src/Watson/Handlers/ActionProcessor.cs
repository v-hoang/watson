using Watson.Abstractions;
using Watson.Models;

namespace Watson.Handlers;

public class ActionProcessor : IActionProcessor
{
    private readonly AudioManager _audioManager;
    private readonly IScriptHandler _scriptHandler;
    private readonly IComputerManager _computerManager;

    public ActionProcessor(AudioManager audioManager,
                           IComputerManager computerManager,
                           IScriptHandler scriptHandler)
    {
        _audioManager = audioManager;
        _computerManager = computerManager;
        _scriptHandler = scriptHandler;
    }

    public bool Process(string filepath, int attempt = 0)
    {
        attempt++;

        try
        {
            var content = File.ReadAllText(filepath);
            var success = ProcessLine(content);

            Console.WriteLine($"Finished processing file with result: {success}");

            return success;
        }
        catch (IOException ex)
        {
            if (attempt < Constants.MaxRetries)
            {
                var delay = Constants.RetryReadDelay * attempt;

                Console.WriteLine($"Failed to read file. Retrying in {delay}ms...");
                Thread.Sleep(delay);
                return Process(filepath, attempt);
            }
            else
            {
                Console.WriteLine($"Could not read file: {filepath}. Error: {ex.Message}. Trace: {ex.StackTrace}");
                throw ex;                
            }
        }
    }

    private bool ProcessLine(string line)
    {
        var arguments = line.Split(Constants.Separators.Action);

        string method, parameter = string.Empty;

        // split line to get action
        Actions action = Enum.TryParse(arguments[0], true, out action) ? action : Actions.Unknown;

        method = arguments.Length > 1 ? arguments[1] : string.Empty;
        parameter = arguments.Length > 2 ? arguments[2] : method;

        Console.WriteLine($"Action: {action}");
        Console.WriteLine($"Method: {method}");
        Console.WriteLine($"Parameter(s): {parameter}");

        switch (action)
        {
            case Actions.Lock:
                _computerManager.Lock();
                break;
            case Actions.Volume:
                return _audioManager.SetVolume(parameter);
            case Actions.Mute:
                _audioManager.Mute();
                break;
            case Actions.ShutDown:
                _computerManager.ShutDown(parameter);
                break;
            case Actions.Module:
                _scriptHandler.LoadModule(method, parameter);
                break;
            case Actions.Function:
                _scriptHandler.Invoke(method, parameter);
                break;
            case Actions.Profile:
                _scriptHandler.InvokeProfile(method, parameter);
                break;
            case Actions.Command:
                _scriptHandler.InvokeCommand($"{method} {parameter}");
                break;
        }

        return true;
    }
}