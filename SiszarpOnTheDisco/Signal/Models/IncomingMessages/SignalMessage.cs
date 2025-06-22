using System.Text.Json.Serialization;

namespace SiszarpOnTheDisco.Signal.Models.IncomingMessages;

public class SignalMessage
{
    [JsonPropertyName("envelope")]
    public Envelope Envelope { get; set; }
    
    [JsonPropertyName("account")]
    public string Account { get; set; }
}