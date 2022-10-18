using System.Threading.Tasks;
using Discord.Commands;
using SiszarpOnTheDisco.Plugins;

namespace SiszarpOnTheDisco.CommandModules;

public class LawnCommands : ModuleBase<SocketCommandContext>
{
    private readonly LawnPlugin _lawnPlugin;

    public LawnCommands(LawnPlugin lawnPlugin)
    {
        _lawnPlugin = lawnPlugin;
    }

    [Command("skoszone", RunMode = RunMode.Async)]
    [Summary("Zapisuje koszenie.")]
    public async Task LawnMowed()
    {
        await ReplyAsync(_lawnPlugin.NewMowingEvent(Context.User.Username, string.Empty).Result);
    }

    [Command("skoszone", RunMode = RunMode.Async)]
    [Summary("Zapisuje koszenie i komentarz.")]
    public async Task LawnMowed([Remainder] [Summary("komentarz")] string comment)
    {
        await ReplyAsync(_lawnPlugin.NewMowingEvent(Context.User.Username, comment).Result);
    }

    [Command("nawiezione", RunMode = RunMode.Async)]
    [Summary("Zapisuje nawożenie")]
    public async Task LawnFertilized()
    {
        await ReplyAsync(_lawnPlugin.NewFertilizingEvent(Context.User.Username, string.Empty).Result);
    }

    [Command("nawiezione", RunMode = RunMode.Async)]
    [Summary("Zapisuje nawożenie i komentarz")]
    public async Task LawnFertilized([Remainder] [Summary("komentarz")] string comment)
    {
        await ReplyAsync(_lawnPlugin.NewFertilizingEvent(Context.User.Username, comment).Result);
    }

    [Command("zgrabione", RunMode = RunMode.Async)]
    [Summary("Zapisuje grabienie")]
    public async Task LawnRaked()
    {
        await ReplyAsync(_lawnPlugin.NewRakingEvent(Context.User.Username, string.Empty).Result); 
    }

    [Command("zgrabione", RunMode = RunMode.Async)]
    [Summary("Zapisuje grabienie i komentarz")]
    public async Task LawnRaked([Remainder] [Summary("komentarz")] string comment)
    {
        await ReplyAsync(_lawnPlugin.NewRakingEvent(Context.User.Username, comment).Result);
    }

    [Command("trawnik", RunMode = RunMode.Async)]
    [Summary("Pobiera status trawnika.")]
    public async Task LawnStatus()
    {
        await ReplyAsync(_lawnPlugin.GetLawnStatus(Context.User.Username));
    }

    [Command("trawnik", RunMode = RunMode.Async)]
    [Summary("Pobiera status trawnika dla danego ogrodnika")]
    public async Task LawnStatus([Remainder] [Summary("nick ogrodnika")] string gardner)
    {
        await ReplyAsync(_lawnPlugin.GetLawnStatus(gardner)); 
    }
}