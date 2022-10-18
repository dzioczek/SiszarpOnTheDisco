using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SiszarpOnTheDisco.Models.HomeAssistant;

public class HomeAssistantEntity
{
    [JsonPropertyName("attributes")] public HomeAssistantEntityAttributes attributes;

    [JsonPropertyName("context")] public Dictionary<string, string> Context;

    [JsonPropertyName("entity_id")] public string entity_id;

    [JsonPropertyName("last_changed")] public DateTime last_changed;

    [JsonPropertyName("last_updated")] public DateTime last_updated;

    [JsonPropertyName("state")] public string state;

    [JsonIgnore] public string StateString => $"{attributes.friendly_name}: {state}{attributes.unit_of_measurement}";

    [JsonIgnore]
    public string Name
    {
        get
        {
            if (attributes.friendly_name != null)
                return attributes.friendly_name;
            return string.Empty;
        }
    }

    [JsonIgnore]
    public bool IsTemperatureSensor
    {
        get
        {
            if (attributes.device_class != null && attributes.device_class.Equals("temperature"))
                return true;
            return false;
        }
    }

    [JsonIgnore]
    public bool IsHumiditySensor
    {
        get
        {
            if (attributes.device_class != null && attributes.device_class.Equals("humidity"))
                return true;
            return false;
        }
    }
}