using Newtonsoft.Json.Linq;

namespace GameServerManagementStudio.Data.JSON.Entities;

public class GameInfoWrapperEntity
{
    public string? EntityType { get; set; }
    public JObject? GameInfo { get; set; }
}