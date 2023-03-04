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
    .ConfigureLogging((context, builder) => {
        // builder.pro]; //"logs/myapp-{Date}.txt"
    })
    .Build();

host.Run();
