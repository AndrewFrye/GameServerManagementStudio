namespace ServerControl;

public class LogEventArgs : EventArgs
{
    public string Message { get; }
    public LogEventArgs(string message)
    {
        Message = message;
    }
}