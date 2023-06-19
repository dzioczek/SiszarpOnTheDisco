using Discord;
using Discord.Interactions;
using SiszarpOnTheDisco.Plugins;
using System.Threading.Tasks;

namespace SiszarpOnTheDisco.CommandModules;

public class LawnCommands : InteractionModuleBase
{
    private readonly LawnPlugin _lawnPlugin;

    public LawnCommands(LawnPlugin lawnPlugin)
    {
        _lawnPlugin = lawnPlugin;
    }

    [SlashCommand("skoszone", "Zapisuje koszenie i komentarz.", runMode: RunMode.Async)]
    public async Task LawnMowed([Summary("komentarz")] string comment = "")
    {
        await RespondAsync(_lawnPlugin.NewMowingEvent(Context.User.Username, comment).Result);
    }

    [SlashCommand("nawiezione", "Zapisuje nawożenie i komentarz", runMode: RunMode.Async)]
    public async Task LawnFertilized([Summary("komentarz")] string comment = "")
    {
        await RespondAsync(_lawnPlugin.NewFertilizingEvent(Context.User.Username, comment).Result);
    }

    [SlashCommand("zgrabione", "Zapisuje grabienie i komentarz", runMode: RunMode.Async)]
    public async Task LawnRaked([Summary("komentarz")] string comment = "")
    {
        await RespondAsync(_lawnPlugin.NewRakingEvent(Context.User.Username, comment).Result);
    }

    [SlashCommand("wykrawedziowane", "Zapisuje krawędziowanie i komentarz.", runMode: RunMode.Async)]
    public async Task LawnEdged([Summary("komentarz")] string comment = "")
    {
        await RespondAsync(_lawnPlugin.NewEdgingEvent(Context.User.Username, comment).Result);
    }

    [SlashCommand("trawnik", "Pobiera status trawnika dla danego ogrodnika", runMode: RunMode.Async)]
    public async Task LawnStatus([Summary("ogrodnik", "Nazwa ogrodnika.")] IUser gardner = null)
    {
        if (gardner == null) gardner = Context.User;

        EmbedBuilder embedBuilder = new()
        {
            Title = $"Status trawnika u {gardner.Username}",
            Description = _lawnPlugin.GetLawnStatus(gardner.Username),
            Color = Color.Green
        }; 
        await RespondAsync(embed: embedBuilder.Build());
    }
}