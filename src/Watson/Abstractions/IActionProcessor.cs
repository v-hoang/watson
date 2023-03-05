namespace Watson.Abstractions;
public interface IActionProcessor
{
    bool Process(string file, int attempt = 0);
}