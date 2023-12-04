using Discord;
using Discord.Interactions;
using SiszarpOnTheDisco.Models.MusicLinks;
using SiszarpOnTheDisco.Plugins;
using System;
using System.Threading.Tasks;

namespace SiszarpOnTheDisco.CommandModules;

public class MusicCommands : InteractionModuleBase
{
    private readonly ulong _musicChannelId;
    private readonly MusicLinksPlugin _musicLinksPlugin;

    public MusicCommands(MusicLinksPlugin musicLinksPlugin)
    {
        _musicLinksPlugin = musicLinksPlugin;
        _musicChannelId = ulong.Parse(Environment.GetEnvironmentVariable("MUSIC_CHANNEL_ID"));
    }

    [SlashCommand("dajseta", "Losuje seta z bazy", runMode: RunMode.Async)]
    public async Task GetRandomLink()
    {
        MusicLink musicLink = _musicLinksPlugin.GetRandomLink();
        EmbedBuilder builder = new()
        {
            Title = musicLink.url,
            Url = musicLink.url,
            Description = musicLink.Stats,
            Color = Color.Red
        };
        await RespondAsync(embed: builder.Build());
    }

    [SlashCommand("dejkrisa", "No chyba wiadomo...")]
    public async Task Manieczki()
    {
        await RespondAsync("A masz se posłuchaj... https://www.youtube.com/watch?v=N1KbTCS-Sz8:");
    }

    [SlashCommand("dodajseta", "Dodaje seta do bazy. Podaj link do seta.", runMode: RunMode.Async)]
    public async Task SaveSet(string link)
    {
        string response = _musicLinksPlugin.AddNewLink(link);
        await RespondAsync(response);

        if (!response.Contains("Było!") & !Context.Channel.Id.Equals(_musicChannelId))
        {
            IGuild guild = Context.Guild;
            ITextChannel textChannel = await guild.GetTextChannelAsync(_musicChannelId);

            await textChannel.SendMessageAsync($"{Context.User.Username} dodał: {response} link: {link}");
        }
    }

    [SlashCommand("szukajseta", "Szuka seta w bazie. Słowa kluczowe rozdzielone spacjami. Szuka w tytułach i tagach.", runMode: RunMode.Async)]
    public async Task FindSet([Summary("query")] string args)
    {
        EmbedBuilder builder = new()
        {
            Description = _musicLinksPlugin.SearchSet(args),
            Color = Color.Red
        };

        await RespondAsync(embed: builder.Build());
    }


    [SlashCommand("yay", "Głosuj na tak!", runMode: RunMode.Async)]
    public async Task VoteYay()
    {
        bool result = await Task.FromResult(_musicLinksPlugin.Vote(true));

        if (result) await RespondAsync("Cieszę się, że się podobało!");
        else await RespondAsync("Nie było nic do głosowania.");
    }

    [SlashCommand("meh", "Głosuj na nie :(", runMode: RunMode.Async)]
    public async Task VoteMeh()
    {
        bool result = await Task.FromResult(_musicLinksPlugin.Vote(false));

        if (result) await RespondAsync("Może następnym razem...");
        else await RespondAsync("Nie było nic do głosowania.");
    }

    [SlashCommand("tagujseta", "Tagowanie seta. Podaj link i tagi rozdzielone spacjami.", runMode: RunMode.Async)]
    public async Task TagSet([Summary("link", "Link seta do otagowania.")] string url, [Summary("tags", "Tagi rozdzielone spacjami.")] string tags)
    {
        await RespondAsync(_musicLinksPlugin.TagSet(url, tags));
    }

    [SlashCommand("usunseta", "Usuwa seta z bazy.", runMode: RunMode.Async)]
    public async Task DeleteSet(string link)
    {
        await RespondAsync(_musicLinksPlugin.DeleteSet(link).Result);
    }

    [SlashCommand("dajamber", "też chyba wiadomo...")]
    public async Task GetAmber()
    {
        EmbedBuilder builder = new()
        {
            Description = _musicLinksPlugin.SearchSet("amber"),
            Color = Color.Red
        };

        await RespondAsync(embed: builder.Build());
    }
}