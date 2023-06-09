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
}

