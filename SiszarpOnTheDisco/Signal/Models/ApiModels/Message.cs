using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SiszarpOnTheDisco.Signal.Models.ApiModels;

public class Message
{
    [JsonPropertyName("message")] public string? Text { get; set; }
    [JsonPropertyName("notify_self")] public bool NotifySelf { get; set; } = true;
    [JsonPropertyName("number")] public string? Number { get; set; } = "+48451165331";
    [JsonPropertyName("recipients")] public List<string> Recipients { get; set; } = new();
    [JsonPropertyName("text_mode")] public string? TextMode { get; set; } = "normal";

    [JsonPropertyName("base64_attachments")]
    public List<string> Attachments { get; set; } = [];
}

    