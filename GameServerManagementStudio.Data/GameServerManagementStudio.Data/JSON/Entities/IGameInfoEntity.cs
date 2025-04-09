namespace GameServerManagementStudio.Data.JSON.Entities;

public interface IGameInfoEntity
{
    public String GameName { get; set; }
    public String GameVersion { get; set; }
    public String StartupApplication { get; set; }
}