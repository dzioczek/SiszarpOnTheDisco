using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SiszarpOnTheDisco.Signal.Models.IncomingMessages;

public class RpcResponse<T,TR>
{
    [JsonPropertyName("jsonrpc")] public string? Jsonrpc { get; set; } = "2.0";

    [JsonPropertyName("method")] public string? Method { get; set; } = string.Empty;


    [JsonPropertyName("params")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T Params { get; set; }

    [JsonPropertyName("result")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TR Result { get; set; }


    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RpcError Error { get; init; }

    [JsonPropertyName("id")] public string? Id { get; set; } = Guid.CreateVersion7().ToString();
}