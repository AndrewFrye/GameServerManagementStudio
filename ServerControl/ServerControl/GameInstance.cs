using System.Diagnostics;

namespace ServerControl;

public enum GameState
{
    Stopped,
    Starting,
    Running,
    Stopping
}

public class GameInstance
{
    public Process? GameProcess { get; set; }
    public string Name { get; set; }
    private readonly ILogger<Worker> _logger;
    public string Id { get; set; }

    public GameState State { get; set; }
    
    public GameInstance(ILogger<Worker> logger, bool startOnCreate = false)
    {
        _logger = logger;
        State = GameState.Stopped;
        Id = Guid.NewGuid().ToString();
    }

    public async Task Start()
    {
        State = GameState.Starting;
        
        //Server startup process
        
        State = GameState.Running;
    }
}