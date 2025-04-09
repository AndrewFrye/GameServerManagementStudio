namespace GameServerManagementStudio.Data.JSON.Entities;

public interface IGameInfoEntity
{
    public  String GameName { get; protected set; }
    public String GameVersion { get; protected set; }
    public String StartupApplication { get; protected set; }
    public String InstanceId { get; protected set; }
    public String InstanceDir { get; protected set; }
    public String? ProcessType { get; protected set; }

    public void Init();
}