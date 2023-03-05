using Microsoft.Extensions.Configuration;

namespace Watson.Tests;

public class BaseTests
{
    protected readonly ITestOutputHelper _output;
    protected IConfiguration _configuration;

    public BaseTests(ITestOutputHelper output)
    {
        _output = output;

        var defaultSettings = new Dictionary<string, string?> { { "TopLevelKey", "TopLevelValue" } };

        _configuration = ConfigureConfiguration(defaultSettings);
    }

    public IConfiguration ConfigureConfiguration(IEnumerable<KeyValuePair<string, string?>>? settings)
    {
        var config = new ConfigurationBuilder();
        
        config.AddInMemoryCollection(settings);

        return config.Build();
    }
}
