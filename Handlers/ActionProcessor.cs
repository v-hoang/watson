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

    public bool ProcessLine(string line)
    {
        var arguments = line.Split(Constants.Separators.Action);

        string method, parameter = string.Empty;

        // split line to get action
        Actions action = Enum.TryParse(arguments[0], true, out action) ? action : Actions.Unknown;

        method = arguments[1];
        parameter = arguments.Length > 2 ? arguments[2] : arguments[1];

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