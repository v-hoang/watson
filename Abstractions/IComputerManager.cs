
namespace Watson.Abstractions;

public interface IComputerManager {
    void ShutDown(string parameter);
    void Sleep();
    bool Lock();
}