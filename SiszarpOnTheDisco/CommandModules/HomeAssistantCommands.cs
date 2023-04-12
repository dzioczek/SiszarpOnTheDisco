using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Serilog;
using SiszarpOnTheDisco.Plugins;

namespace SiszarpOnTheDisco.CommandModules;

public class HomeAssistantCommands : ModuleBase<SocketCommandContext>
{
    private readonly HomeAssistantPlugin _homeAssistant;
    private DateTime _lastPowerCommandTime;
    private ILogger _logger;

    public HomeAssistantCommands(HomeAssistantPlugin homeAssistant, ILogger logger)
    {
        _homeAssistant = homeAssistant;
        _logger = logger;
        _lastPowerCommandTime = DateTime.MinValue;
    }

    [Command("out", RunMode = RunMode.Async)]
    public async Task GetOutsideTemp()
    {
        string reply = _homeAssistant.GetOutsideWeather();
        EmbedBuilder embed = new()
        {
            Title = "Outside temp",
            Description = reply,
            Color = new Color(0x50e3c2),
            Footer = new EmbedFooterBuilder
            {
                Text = $"Response time: {DateTimeOffset.Now - Context.Message.Timestamp:s\\.fff}"
            }
        };

        await Context.Channel.SendMessageAsync(embed: embed.Build());
    }

    [Command("temp", false, RunMode = RunMode.Async)]
    public async Task GetTemps()
    {
        await Context.Channel.SendMessageAsync(_homeAssistant.GetTemperatures());
    }

    [Command("temp", false, RunMode = RunMode.Async)]
    public async Task GetTemps([Remainder] string room)
    {
        await Context.Channel.SendMessageAsync(_homeAssistant.GetRoomTemperature(room));
    }

    [Command("hum")]
    public async Task GetHumidity()
    {
        await Context.Channel.SendMessageAsync(_homeAssistant.GetHumidity());
    }

    [Command("light")]
    public async Task GetLightInfo()
    {
        await Context.Channel.SendMessageAsync(_homeAssistant.GetLight());
    }

    [Command("dryer")]
    public async Task GetDryerInfo()
    {
        await Context.Channel.SendMessageAsync(_homeAssistant.DryerStatus().Result);
    }

    [Command("cam", RunMode = RunMode.Async)]
    public async Task UploadPics()
    {
        List<string> pics = await _homeAssistant.GetLocalPicturePathAsync();
        foreach (string pic in pics) await Context.Channel.SendFileAsync(pic);
    }

    [Command("power", RunMode = RunMode.Async)]
    public async Task GetPowerInfo()
    {
        // if (DateTime.Now < _lastPowerCommandTime) return;
        // _lastPowerCommandTime = DateTime.Now.AddMinutes(5);
        //
        FileInfo file = _homeAssistant.GetPowerChart().Result;
        
        EmbedBuilder embed = new EmbedBuilder
        {
            Title = "Power",
            Description = _homeAssistant.GetPowerStatus().Result,
            Color = Color.Orange,
        
            Footer = new EmbedFooterBuilder
            {
                Text = $"Response time: {DateTimeOffset.Now - Context.Message.Timestamp:s\\.fff}"
            }
        };

        embed.AddField("Dla Okiego:", "14 paneli po 450W co daje 6,3kW");

        if (file == null)
        {
            await Context.Channel.SendMessageAsync(embed: embed.Build());
            return;
        }

        embed.ImageUrl = $"attachment://{file.Name}";
        await Context.Channel.SendFileAsync(file.FullName, embed: embed.Build());
    }

    [Command("cienprosze", RunMode = RunMode.Async)]
    public async Task SetOfficeBlindShade()
    {
        string res = _homeAssistant.SetOfficeBlindShade().Result;
        await Context.Channel.SendMessageAsync($"Staram się: {res}"); 
    }

    [Command("wiecej", RunMode = RunMode.Async)]
    public async Task MoreOfficeShade()
    {
        if (Context.User.Username == "mpDzioczek")
        {
            await ReplyAsync(_homeAssistant.ChangeOfficeBlindShade(true).Result);
        } 
    }
    [Command("mniej", RunMode = RunMode.Async)]
    public async Task LessOfficeShade()
    {
        if (Context.User.Username == "mpDzioczek")
        {
            await ReplyAsync(_homeAssistant.ChangeOfficeBlindShade(false).Result);
        }
    }
    
}