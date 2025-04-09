using GameServerManagementStudio.Data;
using GameServerManagementStudio.Data.JSON.Entities;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Newtonsoft.Json;
using ServerControl.GameProcesses;

namespace ServerControl;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IOMessageBroker _broker;
    private readonly LogHandler _logHandler;

    public Worker(ILogger<Worker> logger, IConfiguration configuration, IOMessageBroker broker)
    {
        _configuration = configuration;
        _logger = logger;
        _broker = broker;
        
        _logHandler = new LogHandler(_logger);
        _logHandler.LogUpdated += OnLogUpdated;
    }
    
    private List<string> _messageQueue = new List<string>();
    private void OnLogUpdated(object? sender, LogEventArgs e)
    {
        _messageQueue.Add(e.Message);
    }
    
    public async Task Startup()
    {
        // This method is called when the application starts

        FetchServerInfos();
        
        await Task.Run(() => _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Startup();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var receiveTask = Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var messageString = await _broker.DequeueInputAsync(stoppingToken);
                    var packet = JsonConvert.DeserializeObject<MessageEntity>(messageString);
                    if (packet == null)
                    {
                        _logger.LogWarning("Received null packet");
                    }
                    
                    _logger.LogInformation("Processing: {message}", packet.Command);

                    // Process IO
                    switch (packet.Command)
                    {
                        case "exit":
                            await _broker.BroadcastOutputAsync("Exiting...");
                            break;
                        case "echo":
                            await EchoMessage(packet.Message);
                            break;
                        case "FetchServerInfos":
                            await SendServerInfos();
                            break;
                        case "Start":
                            await Start(packet.Message);
                            break;
                        default:
                            await _broker.BroadcastOutputAsync($"Unknown command: {packet.Command}");
                            break;
                    }
                }
            }, stoppingToken);

            var sendTask = Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Example: Periodically send a heartbeat or status update
                    while (_messageQueue.Count > 0)
                    {
                        var message = _messageQueue[0];
                        _messageQueue.RemoveAt(0);
                        await _broker.BroadcastOutputAsync(message);
                    }
                    // await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }, stoppingToken);

            await Task.WhenAny(receiveTask, sendTask);
        }
        
        _logger.LogInformation("Worker stopping at: {time}", DateTimeOffset.Now);
        _logHandler.LogUpdated -= OnLogUpdated;
        while (GameProcesses.Count > 0)
        {
            var gameProcess = GameProcesses[0];
            await gameProcess.Stop();
            GameProcesses.RemoveAt(0);
        }
    }
    
    private List<IGameInfoEntity> _gameInfoEntities { get; set; } = new List<IGameInfoEntity>();

    private async Task FetchServerInfos()
    {
        var instanceDirectory = _configuration["InstanceDirectory"];
        if (string.IsNullOrEmpty(instanceDirectory))
        {
            _logger.LogError("Instance directory is not set in the configuration.");
            return;
        }
        
        _logger.LogInformation("Fetching server infos from: {instanceDirectory}", instanceDirectory);

        Matcher matcher = new();
        matcher.AddInclude("**/info.json");
        var results = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(instanceDirectory)));
        
        foreach (var result in results.Files)
        {
            _logger.LogInformation("Found info.json at: {path}", result.Path);
            var jsonString = await File.ReadAllTextAsync(instanceDirectory + result.Path);
            var gameInfoWrapper = JsonConvert.DeserializeObject<GameInfoWrapperEntity>(jsonString);

            if (gameInfoWrapper?.EntityType == null || gameInfoWrapper.GameInfo == null)
            {
                _logger.LogWarning("Invalid or incomplete info.json at: {path}", result.Path);
                continue;
            }
            
            IGameInfoEntity? gameInfoEntity = gameInfoWrapper.EntityType switch
            {
                "MinecraftInfoEntity" => gameInfoWrapper.GameInfo.ToObject<MinecraftInfoEntity>(),
                // Add other entity types here as needed
                _ => null
            };

            if (gameInfoEntity != null)
            {
                gameInfoEntity.Init();
                _gameInfoEntities.Add(gameInfoEntity);
                _logger.LogInformation("Deserialized entity of type {type} from: {path}", gameInfoWrapper.EntityType, result.Path);
            }
            else
            {
                _logger.LogWarning("Unknown entity type {type} in: {path}", gameInfoWrapper.EntityType, result.Path);
            }
        }

        foreach (var instance in _gameInfoEntities)
        {
            _logger.LogInformation("Found instance {instance} in {path}", instance, instanceDirectory);
        }
    }
    
    private async Task SendServerInfos()
    {
        if (_gameInfoEntities.Count == 0)
        {
            _logger.LogWarning("No server info entities found.");
            await _broker.BroadcastOutputAsync("No server info found.");
            return;
        }
        
        var jsonString = JsonConvert.SerializeObject(_gameInfoEntities);
        
        var message = new MessageEntity
        {
            Command = "FetchServerInfos",
            Message = jsonString,
            Source = "HubService"
        };
        await _broker.BroadcastOutputAsync(JsonConvert.SerializeObject(message));
        _logger.LogInformation("Sent server info entities: {entities}", jsonString);
    }

    private async Task EchoMessage(string message)
    {
        await _broker.BroadcastOutputAsync($"Echo: {message}");
    }

    public List<GameProcessBase> GameProcesses { get; set; } = new List<GameProcessBase>();
    private async Task Start(string name)
    {
        var instanceDirectory = _configuration["InstanceDirectory"];
        if (string.IsNullOrEmpty(instanceDirectory))
        {
            _logger.LogError("Instance directory is not set in the configuration.");
            return;
        }
        
        var instance = _gameInfoEntities.FirstOrDefault(x => x.InstanceId == name);
        if (instance == null)
        {
            _logger.LogError("Instance not found: {name}", name);
            await _broker.BroadcastOutputAsync($"Instance not found: {name}");
            return;
        }
        
        var instancePath = Path.Combine(instanceDirectory, instance.InstanceDir);
        if (!Directory.Exists(instancePath))
        {
            _logger.LogError("Instance directory does not exist: {path}", instancePath);
            await _broker.BroadcastOutputAsync($"Instance directory does not exist: {instancePath}");
            return;
        }

        var gameProcessBase = instance.ProcessType switch
        {
            "Minecraft" when instance is MinecraftInfoEntity minecraftEntity => 
                new MinecraftProcess(minecraftEntity, _configuration, _logHandler),
            // Add other process types here as needed
            _ => new GameProcessBase(instance, _configuration, _logHandler)
        };
        
        GameProcesses.Add(gameProcessBase);
        
        
        await gameProcessBase.Start();
    }
    
    private async Task Stop(string name)
    {
        var instance = GameProcesses.FirstOrDefault(x => x.InstanceId == name);
        if (instance == null)
        {
            _logger.LogError("Instance not found: {name}", name);
            await _broker.BroadcastOutputAsync($"Instance not found: {name}");
            return;
        }
        
        await instance.Stop();
    }
}