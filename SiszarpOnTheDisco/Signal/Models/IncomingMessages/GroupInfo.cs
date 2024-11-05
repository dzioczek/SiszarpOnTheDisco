using System.Text.Json.Serialization;

namespace SiszarpOnTheDisco.Signal.Models.IncomingMessages;

public class GroupInfo
{
    [JsonPropertyName("groupId")] public string? GroupId { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }
}