namespace Watson;

public struct Constants
{
    public const int MaxRetries = 10;
    public const int RetryDelay = 50;
    public const string Filter = "*.txt";

    public struct Separators
    {
        public const char Action = '|';
        public const char Time = ' ';
    }

    public struct Commands
    {
        public const string ShutDown = "shutdown";
    }

    public struct Paths
    {
    }
}