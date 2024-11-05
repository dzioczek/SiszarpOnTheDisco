using System.Text.Json.Serialization;

namespace SiszarpOnTheDisco.Signal.Models.IncomingMessages;

public class Envelope
{
    [JsonPropertyName("source")] public string Source { get; set; }

    [JsonPropertyName("sourceNumber")] public string SourceNumber { get; set; }

    [JsonPropertyName("sourceUuid")] public string SourceUuid { get; set; }

    [JsonPropertyName("sourceName")] public string SourceName { get; set; }
    [JsonPropertyName("timestamp")] public long Timestamp { get; set; }

    [JsonPropertyName("dataMessage")] public DataMessage? DataMessage { get; set; }
}