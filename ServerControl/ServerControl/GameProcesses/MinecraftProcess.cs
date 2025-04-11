using GameServerManagementStudio.Data.JSON.Entities;

namespace ServerControl.GameProcesses;

public class MinecraftProcess : GameProcessBase
{
    private new readonly MinecraftInfoEntity _infoEntity;

    public MinecraftProcess(MinecraftInfoEntity infoEntity, IConfiguration config, LogHandler log) :
        base(infoEntity, config, log)
    {
        _infoEntity = infoEntity;
    }

    public override async Task<bool> Start()
    {
        _processStartInfo.FileName = _config[$"JavaPaths:{getJavaVersion()}"];
        _processStartInfo.Arguments =
            $"-Xmx{_infoEntity.MaxMemory}M -Xms{_infoEntity.MinMemory}M {string.Join(" ", _infoEntity.JavaArgs)} -jar {Path.Join(_config["InstanceDirectory"], _infoEntity.InstanceDir, _infoEntity.JarFile)} nogui";

        _log.LogInfo($"Starting Minecraft server process", _infoEntity.InstanceId);

        _process.StartInfo = _processStartInfo;
        var result = await Task.Run(() => _process.Start());

        if (!result)
        {
            _log.LogError($"Failed to start Minecraft server process", _infoEntity.InstanceId);
            return false;
        }

        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        _log.LogInfo($"Minecraft server process started successfully", _infoEntity.InstanceId);
        return true;
    }

    private string getJavaVersion()
    {
        // This method should return the path to the Java executable
        // You can implement logic to find the Java installation on the system
        // For example, you might check common installation paths or use environment variables.

        int javaVersion = 0;

        int majorVersion = int.Parse(_infoEntity.GameVersion.Split('.')[1]);
        if (majorVersion <= 16)
        {
            javaVersion = 8;
        }
        else if (majorVersion <= 17)
        {
            javaVersion = 16;
        }
        else if (majorVersion < 21)
        {
            javaVersion = 17;
        }
        else
        {
            javaVersion = 21;
        }

        if (_infoEntity.JavaVersionOverride != null)
        {
            javaVersion = _infoEntity.JavaVersionOverride ?? javaVersion;
        }


        return $"Java{javaVersion}";
    }
    
    public override async Task SendCommand(string command)
    {
        // Send a command to the game process
        // This is where you would implement the logic to send commands to the game server
        // For example, you might write to the StandardInput stream of the process.
        
        _processInput = _process.StandardInput;
        Console.WriteLine("Sending command: " + command);
        _log.LogInfo($"Sending command: {command}", _infoEntity.InstanceId);
        
        if (!_processInput.BaseStream.CanWrite)
        {
            _log.LogError($"Cannot send command, process input stream is not writable", _infoEntity.InstanceId);
            return;
        }
        
        await _processInput.WriteLineAsync(command);
        _processInput.BaseStream.Flush();
    }
    
    public override async Task Stop()
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