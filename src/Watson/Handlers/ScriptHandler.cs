using System.Management.Automation;
using Watson.Abstractions;
using Watson.Extensions;

namespace Watson.Handlers;

public class ScriptHandler : IScriptHandler
{
    private readonly string _profilePath;
    private readonly string _scriptsPath;

    public ScriptHandler(IConfiguration configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentException("Application cannot start without configuration");
        }

        _scriptsPath = configuration.GetValue<string>("PowershellScripts") ?? $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\Powershell\\Scripts";
        _profilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\Powershell\\Microsoft.PowerShell_profile.ps1";

        Console.WriteLine($"[INFO] Script path: {_scriptsPath}");
        Console.WriteLine($"[INFO] Profile path: {_profilePath}");

        if (!File.Exists(_profilePath) || !Directory.Exists(_scriptsPath))
        {
            Console.WriteLine("[WARN] Scripts might not run properly due to invalid configuration values");
        }
    }

    private string GetFunctionPath(string function) => $"{_scriptsPath}\\{function}.ps1";
    private string GetModulePath(string module) => Path.Combine(_scriptsPath, $"{module}.psm1");

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

    private PowerShell CreateUnrestricted()
    {
        var powershell = PowerShell.Create()
                        .AddCommand(Constants.Commands.SetPolicy)
                        .AddArgument("Unrestricted")
                        .AddParameter("Scope", "CurrentUser");

        powershell.Invoke();

        return powershell;
    }

    // Load module from psm1 file and run method
    // Assumes the module name and command use the same name
    public bool LoadModule(string moduleName = "List-Commands",
                           string argument = "")
    {
        try
        {
            var module = GetModulePath(moduleName);

            if (!File.Exists(module))
            {
                Console.WriteLine("Cannot proceed if module does not exist");
                return false;
            }
                    
            var powershell = CreateUnrestricted().AddCommand("Import-Module")
                    .AddParameter("Name", module)
                    .InvokeLog() // import module only
                    .AddStatement()
                    .AddCommand(moduleName);

            if (string.IsNullOrWhiteSpace(argument))
            {
                powershell.AddArgument(argument);
            }

            powershell.InvokeLog();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to import module. Error: {ex.Message}");
        }

        return false;
    }


    //TODO: Copy individual statement from PS Profile to load all scripts

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

            var results = powershell.InvokeLog();
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