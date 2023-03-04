using System.Diagnostics;
using System.Runtime.InteropServices;
using Watson.Abstractions;

namespace Watson.Handlers;

public class ComputerManager : IComputerManager
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool LockWorkStation();

    public bool Lock()
    {
        try
        {
            return LockWorkStation();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to lock computer. Error: {ex.Message}");
        }
        return false;
    }

    public void ShutDown(string parameter)
    {
        const int SecondsInMinutes = 60;

        var shutdownParameters = string.Empty;
        var message = "Shutting down";

        if (parameter == "abort")
        {
            shutdownParameters = "/a";
            message = "Cancelling shutdown...";
        }
        else
        {
            var items = parameter.Split(Constants.Separators.Time);
            var minutesString = items[items.Length - 1]; //should be last

            int minutes = int.TryParse(minutesString, out minutes) ? minutes : 10;
            shutdownParameters = $"/s /t {minutes * SecondsInMinutes}";
        }

        Console.WriteLine(message);
        Process.Start(Constants.Commands.ShutDown, shutdownParameters);
    }

    public void Sleep()
    {
        throw new NotImplementedException();
    }
}