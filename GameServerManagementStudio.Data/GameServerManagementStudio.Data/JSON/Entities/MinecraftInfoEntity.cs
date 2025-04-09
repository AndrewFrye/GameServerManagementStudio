namespace GameServerManagementStudio.Data.JSON.Entities;

public class MinecraftInfoEntity : IGameInfoEntity
{
    public List<string> JavaArgs { get; set; } = new();
    
    public string GameName = "Minecraft";
    
    
    public MinecraftInfoEntity(string packName, string packVersion, string jar, int javaVersion)
    {
        GameName = packName;
        GameVersion = packVersion;
        StartupApplication = startupApplication;
    }
}