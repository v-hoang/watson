namespace Watson;

public struct Constants
{
    public const int MaxRetries = 10;
    public const int RetryDelay = 50;
    public const string Filter = "*.txt";

    public struct Configuration {
        public const string CleanOnInit = "CleanOnInit";
        public const string WatchedFolder = "WatchedFolder";
    }
    
    public struct Separators
    {
        public const char Action = '|';
        public const char Time = ' ';
    }

    public struct Commands
    {
        public const string SetPolicy = "Set-ExecutionPolicy";
        public const string ShutDown = "shutdown";
    }

    public struct Paths
    {
    }
}