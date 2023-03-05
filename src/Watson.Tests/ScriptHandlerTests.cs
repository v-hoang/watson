using Watson.Handlers;

namespace Watson.Tests;

public class ScriptHandlerTests : BaseTests
{
    private readonly ScriptHandler _handler;

    public ScriptHandlerTests(ITestOutputHelper output) : base(output)
    {
        _handler = new ScriptHandler(_configuration);
    }

    [Theory]
    [InlineData("List-Commands")]
    [InlineData("Shuffle-Name")]
    public void LoadModule_ShouldWork_WhenModuleExists(string module)
    {
        // act
        var success = _handler.LoadModule(module);

        // assert
        Assert.True(success);
        Assert.True(File.Exists(@"C:\Temp\logs\log_list-command.txt"), "The log file was not created!"); // the new module should create this file
    }

    [Fact]
    public void LoadModule_ShouldThrow_WhenInvalidCommand()
    {
        // arrange
        var command = "List-Commandzz";

        // act & assert
        Assert.Throws<Exception>(() =>
        {
            var success = _handler.LoadModule(command);

            _output.WriteLine($"LoadModule resulted in {success}");
        });
    }
}