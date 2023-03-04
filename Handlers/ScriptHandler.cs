using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using Watson.Abstractions;

namespace Watson.Handlers;

public class ScriptHandler : IScriptHandler
{
    private readonly string _profilePath;
    private readonly string _scriptsPath;
    private readonly bool IsDisabled;

    public ScriptHandler(IConfiguration configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentException("Application cannot start without configuration");
        }

        _scriptsPath = configuration.GetValue<string>("PowershellScripts") ?? $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\Powershell\\Scripts";
        _profilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\Powershell\\Microsoft.PowerShell_profile.ps1";

        IsDisabled = !File.Exists(_profilePath);
    }

    private string GetFunctionPath(string function) => $"{_scriptsPath}\\{function}.ps1";

    // Run the Powershell profile init and run the function by name
    public void InvokeProfile(string function, string parameters)
    {
        try
        {
            Console.WriteLine($"Profile path: {_profilePath}");

            var powershell = PowerShell.Create()
            // AddStatement("Set-ExecutionPolicy").AddArgument("Unrestricted -Scope CurrentUser")
                    .AddScript($"& \"{_profilePath}\"")
                    .AddScript(function);

            var results = powershell.Invoke(parameters);

            foreach (var result in results)
            {
                Console.WriteLine(result.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    // Loads the .ps1 file defining the function then invokes it
    public void Invoke(string function, string parameters)
    {
        try
        {
            // scriptInvoker.Invoke("Set-ExecutionPolicy Unrestricted -Scope CurrentUser");

            var functionPath = GetFunctionPath(function);
            var invoke = string.IsNullOrEmpty(parameters) ? function : $"{function} {parameters}";

            Console.WriteLine($"Function path: {functionPath}");
            Console.WriteLine($"Invoke: {invoke}");

            var powershell = PowerShell.Create()
                      .AddScript(functionPath)
                      // .AddScript(invoke);
                      .AddCommand(function)
                      .AddArgument(parameters);

            var results = powershell.Invoke();

            foreach (var result in results)
            {
                Console.WriteLine(result.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public void InvokeCommand(string command)
    {
        try
        {
            var results = PowerShell.Create()
                                    .AddScript(command)
                                    .Invoke();

            foreach (var result in results)
            {
                Console.WriteLine(result.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}