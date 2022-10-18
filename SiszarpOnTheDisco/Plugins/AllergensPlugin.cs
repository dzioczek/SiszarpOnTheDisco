using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using SiszarpOnTheDisco.Models;
using SiszarpOnTheDisco.Models.Allergens;

namespace SiszarpOnTheDisco.Plugins;

public class AllergensPlugin
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger _logger;
    private const string ApiAlertsUrl = "alerts/public/date/{0:dd-MM-yyyy}/region/7";
    private const string ApiBaseUrl = "https://api.zadnegoale.pl/";
    private const string ApiDustsUrl = "dusts/public/date/{0:dd-MM-yyyy}/region/7";

    public AllergensPlugin(ILogger log, ApplicationDbContext dbContext)
    {
        _logger = log;
        _dbContext = dbContext;
    }

    private static string PrepareUrl(string url)
    {
        return string.Format(url, DateTime.Today);
    }

    private async Task<List<Dust>> GetDusts()
    {
        using HttpClient client = new()
        {
            BaseAddress = new Uri(ApiBaseUrl)
        };

        List<Dust> dusts = await client.GetFromJsonAsync<List<Dust>>(PrepareUrl(ApiDustsUrl));

        return dusts;
    }

    public async Task<string> GetAllAllergens()
    {
        List<Dust> dusts = await GetDusts();
        List<AllergenIcon> icons = _dbContext.AllergenIcons.OrderBy(x => x.ID).ToList();

        StringBuilder sb = new();

        foreach (Dust dust in dusts)
        {
            string icon;
            if (icons.Exists(x => x.Name.Equals(dust.allergen.name)))
                icon = icons.First(x => x.Name.Equals(dust.allergen.name)).ToString();
            else
                icon = ":warning:";
            sb.AppendLine($"{icon} {dust.allergen.name}: {dust.level} - {dust.trend}");
        }

        return sb.ToString();
    }

    public async Task<string> GetAlert()
    {
        using HttpClient client = new()
        {
            BaseAddress = new Uri(ApiBaseUrl)
        };

        List<Alert> alerts = await client.GetFromJsonAsync<List<Alert>>(PrepareUrl(ApiAlertsUrl));

        StringBuilder sb = new();

        if (alerts != null)
            foreach (Alert alert in alerts)
                sb.AppendLine(alert.text);

        return sb.ToString();
    }

    public async Task<string> AddIconAsync(string allergen, string icon)
    {
        try
        {
            if (!_dbContext.AllergenIcons.ToList().Exists(x => x.Name.Equals(allergen)))
            {
                AllergenIcon allergenIcon = new()
                {
                    Name = allergen,
                    IconName = icon
                };

                _dbContext.AllergenIcons.Add(allergenIcon);
                await _dbContext.SaveChangesAsync();
                _logger.Information("Added new icon for allergen: {Allergen} - {Icon}", allergen, icon);
                return "success!";
            }

            _logger.Information("Icon for allergen already exists: {Allergen} - {Icon}", allergen, icon);
            return "Already exists.";
        }
        catch
        {
            _logger.Information("issue with adding new icon for allergen: {Allergen} - {Icon}" , allergen, icon);
            return "fail:(";
        }
    }
}