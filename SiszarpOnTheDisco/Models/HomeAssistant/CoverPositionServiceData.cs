using System.Text.Json.Serialization; 

namespace SiszarpOnTheDisco.Models.HomeAssistant;

public class CoverPositionServiceData
{
    [JsonPropertyName("entity_id")]
    public string EntityId { get; set; }
    
    [JsonPropertyName("position")]
    public int Position { get; set; }
}