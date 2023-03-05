using System.Reflection;
using Watson;
using Watson.Abstractions;
using Watson.Handlers;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<IWatcher, Watcher>();
        services.AddSingleton<IActionProcessor, ActionProcessor>();
        services.AddSingleton<IComputerManager, ComputerManager>();
        services.AddSingleton<IScriptHandler, ScriptHandler>();
        services.AddSingleton<AudioManager>();
        services.AddHostedService<Worker>();
    })
    .ConfigureAppConfiguration(builder =>
    {
        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); //Path.Combine(Directory.GetCurrentDirectory(), @"src\watson");

        builder.SetBasePath(basePath)
               .AddJsonFile("appsettings.json", false, true)
#if DEBUG
               .AddJsonFile("appsettings.Development.json", true, true)
#endif
               .AddEnvironmentVariables();
    })
    .ConfigureLogging((context, builder) =>
    {
        // builder.pro]; //"logs/myapp-{Date}.txt"
    })
    .Build();

host.Run();
