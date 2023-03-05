using System.Management.Automation;

namespace Watson.Extensions;

public static class PowershellExtensions
{
    public static PowerShell InvokeLog(this PowerShell powershell)
    {
        try
        {
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

        return powershell;
    }
}