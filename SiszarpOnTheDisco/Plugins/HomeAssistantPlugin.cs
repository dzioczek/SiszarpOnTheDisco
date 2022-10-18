using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Serilog;
using SiszarpOnTheDisco.Models.HomeAssistant;

namespace SiszarpOnTheDisco.Plugins;

public class HomeAssistantPlugin
{
    private readonly string _apiUrl;
    private readonly string _apiKey;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _serializerOptions;
    
    public HomeAssistantPlugin(ILogger logger)
    {
        _logger = logger;
        _apiKey = Environment.GetEnvironmentVariable("HA_API_KEY");
        _apiUrl = Environment.GetEnvironmentVariable("HA_API_BASE_URL");
        
        _serializerOptions = new()
        {
            WriteIndented = true,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            IncludeFields = true
        };

    }

    public string GetTemperatures()
    {
        HomeAssistantEntity[] entities = GetEntities().Result;

        StringBuilder sb = new();

        foreach (HomeAssistantEntity entity in entities.Where(x => x.IsTemperatureSensor.Equals(true)))
            sb.AppendLine(entity.StateString);

        return sb.ToString();
    }

    
    public string GetRoomTemperature(string room)
    {
        HomeAssistantEntity[] entities = GetEntities().Result;

        StringBuilder sb = new();

        foreach (HomeAssistantEntity entity in entities.Where(x => x.IsTemperatureSensor.Equals(true))
                     .Where(x => x.Name.Contains(room, StringComparison.OrdinalIgnoreCase)).ToList())
            sb.AppendLine(entity.StateString);

        return sb.ToString() == string.Empty ? "Room not found" : sb.ToString();
    }

    public string GetHumidity()
    {
        HomeAssistantEntity[] entities = GetEntities().Result;

        StringBuilder sb = new();

        foreach (HomeAssistantEntity entity in entities.Where(x => x.IsHumiditySensor.Equals(true)))
            sb.AppendLine(entity.StateString);

        return sb.ToString();
    }

    public string GetOutsideWeather()
    {
        HomeAssistantEntity[] entities = GetEntities().Result;

        StringBuilder sb = new();

        sb.AppendLine(entities
            .First(x => x.entity_id.Contains("outside_temperature", StringComparison.OrdinalIgnoreCase)).StateString);
        sb.AppendLine(entities.First(x => x.entity_id.Contains("outside_humidity", StringComparison.OrdinalIgnoreCase))
            .StateString);

        HomeAssistantEntity batteryEntity =
            entities.First(x => x.entity_id.Contains("outside_battery", StringComparison.OrdinalIgnoreCase));

        if (int.Parse(batteryEntity.state) < 30)
            sb.AppendLine(batteryEntity.StateString);

        return sb.ToString();
    }

    public string GetLight()
    {
        HomeAssistantEntity[] entities = GetEntities().Result;
        return entities.First(x =>
            x.entity_id.Contains("light_sensor_illuminance_lux", StringComparison.OrdinalIgnoreCase)).StateString;
    }

    public async Task<string> DryerStatus()
    {
        try
        {
            HomeAssistantEntity[] entities = await GetEntities();
            StringBuilder sb = new();

            sb.AppendLine(entities.FirstOrDefault(e => e.entity_id.Contains("switch.dryer_power"))?.StateString);
            sb.AppendLine(entities.FirstOrDefault(e => e.entity_id.Contains("sensor.dryer_operation_state"))
                ?.StateString);

            if (entities.FirstOrDefault(e => e.entity_id.Contains("dryer_program") && e.state.Contains("on")) != null)
                sb.AppendLine(entities
                    .FirstOrDefault(e => e.entity_id.Contains("dryer_program") && e.state.Contains("on"))?.StateString);

            HomeAssistantEntity remainingTime =
                entities.FirstOrDefault(e => e.entity_id.Contains("sensor.dryer_remaining_program_time"));
            DateTime endTime;
            DateTime.TryParse(remainingTime.state, out endTime);
            TimeSpan ts = endTime.ToUniversalTime() - DateTime.UtcNow;

            sb.AppendLine($"{remainingTime.attributes.friendly_name} {ts.ToString(@"hh\:mm")}");

            sb.AppendLine(entities.FirstOrDefault(e => e.entity_id.Contains("sensor.dryer_duration"))?.StateString);

            string doorStatus = entities.FirstOrDefault(e => e.entity_id.Contains("binary_sensor.dryer_door"))
                ?.StateString;
            doorStatus = Regex.Replace(doorStatus, "off", "closed");
            doorStatus = Regex.Replace(doorStatus, "on", "open");
            sb.AppendLine(doorStatus);

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return string.Format($"{ex.Message} - {ex.StackTrace}");
        }
    }

    public async Task<string> GetPowerStatus()
    {
        try
        {
            HomeAssistantEntity[] entities = await GetEntities();
            StringBuilder sb = new();

            sb.AppendLine(
                entities.FirstOrDefault(x => x.entity_id == "sensor.growatt_output_watt_actual")?.StateString);
            sb.AppendLine(
                entities.FirstOrDefault(x => x.entity_id == "sensor.growatt_generated_energy_today")?.StateString);
            sb.AppendLine(entities.FirstOrDefault(x => x.entity_id == "sensor.monthly_energy")?.StateString);
            sb.AppendLine(entities.FirstOrDefault(x => x.entity_id == "sensor.yearly_energy")?.StateString);
            sb.AppendLine(entities.FirstOrDefault(x => x.entity_id == "sensor.growatt_generated_energy_total")
                ?.StateString);

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return string.Format($"{ex.Message} - {ex.StackTrace}");
        }
    }

    public async Task<List<string>> GetLocalPicturePathAsync()
    {
        List<HomeAssistantEntity> cameras = GetCameraEntities();
        List<string> picturesPaths = new();

        foreach (HomeAssistantEntity camera in cameras)
        {
            Task<string> savePicture = SavePictureAsync(CreatePictureAddress(camera));
            string path = await savePicture;
            if (!path.Equals("error"))
                picturesPaths.Add(path);
        }

        return picturesPaths;
    }

    private async Task<string> SavePictureAsync(string pictureUrl)
    {
        using HttpClient client = new();
        client.DefaultRequestHeaders.Add("User-Agent", "Anything");
        HttpResponseMessage response = await client.GetAsync(pictureUrl);

        if (response.StatusCode.Equals(HttpStatusCode.OK))
        {
            Stream remoteStream = await response.Content.ReadAsStreamAsync();

            FileStream diskStream = File.Create(CreatePictureLocalPath());

            remoteStream.Seek(0, SeekOrigin.Begin);
            remoteStream.CopyTo(diskStream);
            diskStream.Close();
            return diskStream.Name;
        }

        return "error";
    }

    private string CreatePictureAddress(HomeAssistantEntity camera)
    {
        return $"{_apiUrl}camera_proxy/{camera.entity_id}?token={camera.attributes.access_token}";
    }

    private string CreatePictureLocalPath()
    {
        // FileInfo executingProgram = new FileInfo(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
        DirectoryInfo directoryInfo = new(Path.Combine("/", "pics", "cameras"));

        if (!directoryInfo.Exists) directoryInfo.Create();

        long timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        return Path.Combine(directoryInfo.FullName, $"{timeStamp}.jpg");
    }


    public List<HomeAssistantEntity> GetCameraEntities()
    {
        HomeAssistantEntity[] entities = GetEntities().Result;
        return entities.Where(x => x.entity_id.Contains("camera.", StringComparison.OrdinalIgnoreCase)).ToList();
    }
    
    public async Task<HomeAssistantEntity[]> GetEntities()
    {
        using HttpClient client = new();

        client.DefaultRequestHeaders.Add("User-Agent", "Anything");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        string url = $"{_apiUrl}states";

        return await client.GetFromJsonAsync<HomeAssistantEntity[]>(url, _serializerOptions);    
    }

    public async Task<string> SetOfficeBlindShade()
    {
        return await SetBlindPosition(new CoverPositionServiceData()
            { EntityId = "cover.blind_theoffice", Position = 50 });
    }

    public async Task<string> ChangeOfficeBlindShade(bool more)
    {
        HomeAssistantEntity officeBlind =
            GetEntities().Result.FirstOrDefault(x => x.entity_id == "cover.blind_theoffice");

        // 100 is fully open and 0 is fully closed
        // more shade is less open hence you have to lower number
        
        if (officeBlind != null)
        {
            int direction = 1;
            switch (more)
            {
                case true when officeBlind.attributes.current_position <= 0:
                    return "Bardziej się nie da!"; 
                case true:
                    direction = -1;
                    break;
                case false when officeBlind.attributes.current_position >= 100:
                    return "Bardziej się nie da!";
            }

            int newPosition = officeBlind.attributes.current_position + (direction * 10);
            await SetBlindPosition(new CoverPositionServiceData()
                { EntityId = "cover.blind_theoffice", Position = newPosition });
            return $"Ustawiam roletę na {newPosition}";
        }

        return "Cannot retrieve entity data.";
    }

    public async Task<string> SetBlindPosition(CoverPositionServiceData serviceData)
    {
        using (HttpClient client = new())
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Anything");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string url = $"{_apiUrl}services/cover/set_cover_position";
            HttpResponseMessage response = await client.PostAsJsonAsync(url, serviceData);
            return response.StatusCode.ToString();
        }
    }

    public async Task<FileInfo> GetPowerChart()
    {
        try
        {
            List<HomeAssistantEntity> entities = await GetPowerHistoryEntities();

            //Dictionary<int, double> data = new Dictionary<int, double>();
            Dictionary<DateTime, double> data = new();

            foreach (HomeAssistantEntity entity in entities)
                //if (!data.Keys.Contains(entity.last_updated.Hour)) data.Add(entity.last_updated.Hour, 0);
                //data[entity.last_updated.Hour] += double.Parse(entity.state, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture) / 1000;
                if (entity.last_updated.TimeOfDay != TimeSpan.Zero)
                    data.Add(entity.last_updated,
                        double.Parse(entity.state, NumberStyles.Float | NumberStyles.AllowThousands,
                            CultureInfo.InvariantCulture) / 1000);

            LiveChartsGenerator liveChartsGenerator = new();
            return new FileInfo(
                await liveChartsGenerator.GetPowerChartPath(data.Keys.ToArray(), data.Values.ToArray()));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Exception getting power chart");
            return null;
        }
    }


    private async Task<List<HomeAssistantEntity>> GetPowerHistoryEntities()
    {
        DateTime startTime = new(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, Calendar.CurrentEra);

        string powerHistoryUrl =
            $"history/period/{startTime:yyyy-MM-ddTHH:mm:ss+00:00}?filter_entity_id=sensor.growatt_output_watt_actual";
        // powerHistoryUrl = $"history/period?filter_entity_id=sensor.my_plant_total_output_power";
        using (HttpClient client = new())
        {
            client.BaseAddress = new Uri(_apiUrl);
            client.DefaultRequestHeaders.Add("User-Agent", "Anything");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            List<List<HomeAssistantEntity>> res =
                await client.GetFromJsonAsync<List<List<HomeAssistantEntity>>>(powerHistoryUrl, _serializerOptions);

            return res.FirstOrDefault();
        }
    }
}