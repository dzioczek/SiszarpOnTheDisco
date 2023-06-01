using Discord;
using Discord.Interactions;
using SiszarpOnTheDisco.Models.MusicLinks;
using SiszarpOnTheDisco.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiszarpOnTheDisco.CommandModules;

public class TodoCommands : InteractionModuleBase
{
    private readonly LawnPlugin _lawnPlugin;
    private readonly MusicLinksPlugin _musicLinksPlugin;
    public TodoCommands(LawnPlugin lawnPlugin, MusicLinksPlugin musicLinksPlugin)
    {
        _lawnPlugin = lawnPlugin;
        _musicLinksPlugin = musicLinksPlugin;
    }

    [SlashCommand("echo", "Echo test")]
    public async Task Echo(string input)
    {
        await Context.Channel.SendMessageAsync(input);
        await RespondAsync(input);

    }

    [SlashCommand("trawnik", "Pobiera status trawnika dla danego ogrodnika", runMode: RunMode.Async)]
    public async Task LawnStatus([Summary("ogrodnik", "Nazwa ogrodnika.")] IUser gardner)
    {
        await RespondAsync(_lawnPlugin.GetLawnStatus(gardner.Username));
    }


    [SlashCommand("dajseta", "Losuje seta z bazy", runMode: RunMode.Async)]
    public async Task GetRandomLink()
    {
        StringBuilder stringBuilder = new();
        // stringBuilder.AppendLine($"Message time: {Context.c.Timestamp.ToString()}");

        //stringBuilder.AppendLine($"before getting link: {DateTimeOffset.Now.ToString()}");
        MusicLink musicLink = _musicLinksPlugin.GetRandomLink();
        //stringBuilder.AppendLine($"after getting link: {DateTimeOffset.Now.ToString()}");
        stringBuilder.AppendLine(musicLink.Stats);
        EmbedBuilder builder = new()
        {
            Title = musicLink.url,
            Url = musicLink.url,
            Description = stringBuilder.ToString(),
            Color = Color.Red
        };
        await RespondAsync(embed: builder.Build());
    }
}

