namespace GameServerManagementStudio.Data.JSON.Entities;

/// <summary>
/// Info entity for Minecraft instances, this is designed to handle modded instances with various java versions
/// </summary>
public class MinecraftInfoEntity : IGameInfoEntity
{
    public List<string> JavaArgs { get; set; } = new();
    
    public string GameName { get; set; }
    public string GameVersion { get; set; }
    public string StartupApplication { get; set; }
    public string PackName { get; set; }
    public string PackVersion { get; set; }
    public int? JavaVersionOverride { get; set; }
    public List<string>? MinecraftArgs { get; set; } = new();
    public string InstanceId { get; set; }
    public string InstanceDir { get; set; }
    public string? ProcessType { get; set; } = "Minecraft";
    public int MaxMemory { get; set; } = 8192;
    public int MinMemory { get; set; } = 1024;
    public string JarFile { get; set; } = "server.jar";

    public void Init()
    {
        if (string.IsNullOrEmpty(InstanceId))
            InstanceId = $"{GameName}_{PackName}";
    }
    
}