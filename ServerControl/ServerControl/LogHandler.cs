using GameServerManagementStudio.Data;
using Newtonsoft.Json;

namespace ServerControl;

public class LogHandler
{
    private readonly ILogger<Worker> _logger;
    private readonly int _maxMessages;
    
    public LogHandler(ILogger<Worker> logger, int maxMessages = 100)
    {
        _maxMessages = maxMessages;
        _logger = logger;
    }
    public List<string> LogMessages { get; private set; } = new List<string>();

    private string encodeMessage(string message, string sender, string flag)
    {
        var messageEntity = new MessageEntity();
        messageEntity.Source = sender;
        messageEntity.Message = $"[{DateTime.UtcNow}] [{flag}] {message}";
        messageEntity.Command = "Log";
        
        return JsonConvert.SerializeObject(messageEntity);
    }
    public void LogInfo(string message, string sender)
    {
        var encodedMessage = encodeMessage(message, sender, "INFO");
        LogMessages.Add(encodedMessage);
        _logger.LogInformation(encodedMessage);
        LogUpdated?.Invoke(this, new LogEventArgs(encodedMessage));
        Update();
    }
    
    public void LogWarning(string message, string sender)
    {
        var encodedMessage = encodeMessage(message, sender, "WARNING");
        LogMessages.Add(encodedMessage);
        _logger.LogWarning(encodedMessage);
        LogUpdated?.Invoke(this, new LogEventArgs(encodedMessage));
        Update();
    }
    
    public void LogError(string message, string sender)
    {
        var encodedMessage = encodeMessage(message, sender, "ERROR");
        LogMessages.Add(encodedMessage);
        _logger.LogError(encodedMessage);
        LogUpdated?.Invoke(this, new LogEventArgs(encodedMessage));
        Update();
    }
    
    public EventHandler<LogEventArgs>? LogUpdated;

    private void Update()
    {
        if (LogMessages.Count > _maxMessages)
        {
            LogMessages.RemoveRange(0, LogMessages.Count - _maxMessages);
        }
    }
}

