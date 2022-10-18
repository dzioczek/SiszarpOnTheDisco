using System;
using System.Text.Json.Serialization;

namespace SiszarpOnTheDisco.Models.HomeAssistant;

public class HomeAssistantEntityAttributes
{
    [JsonPropertyName("access_token")] public string access_token;

    [JsonPropertyName("device_class")] public string device_class;

    [JsonPropertyName("entity_picture")] public string entity_picture;

    [JsonPropertyName("friendly_name")] public string friendly_name;

    [JsonPropertyName("icon")] public string icon;

    [JsonPropertyName("on")] public string on;

    [JsonPropertyName("unit_of_measurement")]
    public string unit_of_measurement;
    
    [JsonPropertyName("current_position")] public int current_position { get; set; }
}