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
}