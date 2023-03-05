namespace Watson.Abstractions;

public interface IScriptHandler {
    bool LoadModule(string moduleName, string argument = "");
    void InvokeCommand(string command);
    void Invoke(string function, string parameters);
    void InvokeProfile(string function, string parameters);
}