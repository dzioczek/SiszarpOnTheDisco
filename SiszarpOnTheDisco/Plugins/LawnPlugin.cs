using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using SiszarpOnTheDisco.Models;
using SiszarpOnTheDisco.Models.Lawn;

namespace SiszarpOnTheDisco.Plugins;

public class LawnPlugin
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger _logger;

    public LawnPlugin(ApplicationDbContext db, ILogger logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<string> NewMowingEvent(string user, string comment)
    {
        return await CreateLawnEvent(user, comment, LawnEventTypes.Mowing);
    }

    public async Task<string> NewFertilizingEvent(string user, string comment)
    {
        return await CreateLawnEvent(user, comment, LawnEventTypes.Fertilizing);
    }

    public async Task<string> NewRakingEvent(string user, string comment)
    {
        return await CreateLawnEvent(user, comment, LawnEventTypes.Raking);
    }

    public async Task<string> NewEdgingEvent(string user, string comment)
    {
        return await CreateLawnEvent(user, comment, LawnEventTypes.Edging);
    }

    private async Task<string> CreateLawnEvent(string user, string comment, LawnEventTypes eventType)
    {
        try
        {
            LawnEvent lawnEvent = new()
            {
                Gardner = user,
                Comment = comment,
                EventDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EventType = eventType
            };

            _db.LawnEvents.Add(lawnEvent);
            await _db.SaveChangesAsync();
            return comment != "" ? $"Zapisane: {comment}" : "Zapisane!"; 
        }
        catch (Exception e)
        {
            _logger.Error("Issue creating new LawnEvent in DB {Message}", e.Message);
            return "Coś poszło nie tak :(";
        }
    }

    public string GetLawnStatus(string user)
    {
        List<LawnEvent> lawnEvents = new();

        foreach (LawnEventTypes type in LawnEventTypes.List)
        {
            lawnEvents.Add(GetLatestEvent(user, type)); 
        }

        if (lawnEvents.All(x => x is null)) return "Nic nie znalazłem :(";
        
        StringBuilder sb = new StringBuilder();
        
        lawnEvents.Sort();
        foreach (LawnEvent lawnEvent in lawnEvents.Where(x => x is not null))
        {
            sb.AppendLine(lawnEvent.ToString());
        }
        
        return sb.ToString();
    }

    private LawnEvent GetLatestEvent(string user, LawnEventTypes type)
    {
        try
        {
            return _db.LawnEvents
                .Where(x => x.Gardner == user && x.EventType == type)
                .OrderByDescending(x => x.EventDate)
                .FirstOrDefault();
        }
        catch (Exception e)
        {
            _logger.Error("Issue getting LawnEvents for {User}: {Message}", user, e.Message);
            return null;
        }
    }
}