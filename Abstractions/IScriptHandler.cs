namespace Watson.Abstractions;

public interface IScriptHandler {
    void InvokeCommand(string command);
    void Invoke(string function, string parameters);
    void InvokeProfile(string function, string parameters);
}