using System.Text.Json.Serialization;

namespace SiszarpOnTheDisco.Signal.Models.IncomingMessages;

public class RpcError
{
    [JsonPropertyName("code")] public int Code { get; set; }

    [JsonPropertyName("message")] public string Message { get; set; }

    [JsonIgnore]
    [JsonPropertyName("data")]
    public object Data { get; set; }

}