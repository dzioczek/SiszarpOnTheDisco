using System.Text.Json.Serialization;

namespace SiszarpOnTheDisco.Signal.Models.IncomingMessages;

public class DataMessage
{
    
    [JsonPropertyName("timestamp")] public long Timestamp { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("expiresInSeconds")] public int ExpiresInSeconds { get; set; }
    [JsonPropertyName("viewOnce")] public bool ViewOnce { get; set; }
    [JsonPropertyName("groupInfo")] public GroupInfo? GroupInfo { get; set; }
}