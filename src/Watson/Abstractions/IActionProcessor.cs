namespace Watson.Abstractions;
public interface IActionProcessor
{
    bool ProcessLine(string line);
}