using System.Diagnostics;
using GameServerManagementStudio.Data.JSON.Entities;

namespace ServerControl.GameProcesses;

public class GameProcessBase
{
    protected readonly IGameInfoEntity _infoEntity;
    protected readonly IConfiguration _config;
    protected readonly LogHandler _log;
    protected Process _process;
    protected ProcessStartInfo _processStartInfo;
    public string InstanceId => _infoEntity.InstanceId;
    public bool Running { get; private set; } = false;
    protected StreamWriter _processInput;
    
    public GameProcessBase(IGameInfoEntity infoEntity, IConfiguration config, LogHandler log)
    {
        _infoEntity = infoEntity;
        _config = config;
        _log = log;
        _process = new Process();
        _processStartInfo = new ProcessStartInfo
        {
            FileName = Path.Join(_config["InstanceDirectory"], _infoEntity.InstanceDir, _infoEntity.StartupApplication),
            WorkingDirectory = Path.Join(_config["InstanceDirectory"], _infoEntity.InstanceDir),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        
        _process.OutputDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                _log.LogInfo(args.Data, _infoEntity.InstanceId);
            }
        };
        _process.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                _log.LogError(args.Data, _infoEntity.InstanceId);
            }
        };
    }
    
    public virtual async Task<bool> Start()
    {
        // Start the game process
        // This is where you would implement the logic to start the game server
        // For example, you might use Process.Start() to launch the server executable
        // and pass any necessary arguments.
        
        _process.StartInfo = _processStartInfo;
        _log.LogInfo($"Starting game process", _infoEntity.InstanceId);
        
        var result = await Task.Run(() => _process.Start());
        if (!result)
        {
            _log.LogError($"Failed to start game process", _infoEntity.InstanceId);
            return false;
        }
        
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
        
        _log.LogInfo($"Game process started successfully", _infoEntity.InstanceId);
        Running = true;
        return true;
    }
    
    public virtual async Task SendCommand(string command)
    {
        // Send a command to the game process
        // This is where you would implement the logic to send commands to the game server
        // For example, you might write to the StandardInput stream of the process.
        
        _processInput = _process.StandardInput;

        
        if (!_processInput.BaseStream.CanWrite)
        {
            _log.LogError($"Cannot send command, process input stream is not writable", _infoEntity.InstanceId);
            return;
        }
        
        await _processInput.WriteLineAsync(command);
        _processInput.BaseStream.Flush();
    }
    
    public virtual async Task Stop()
    {
        // Stop the game process
        // This is where you would implement the logic to stop the game server
        // For example, you might use Process.Kill() to terminate the server process.
        
        if (_process != null && !_process.HasExited)
        {
            _log.LogInfo($"Stopping game process", _infoEntity.InstanceId);
            await SendCommand("stop");
            await Task.Run(() => _process.WaitForExit());
            _log.LogInfo($"Game process stopped successfully", _infoEntity.InstanceId);
        }
    }
}