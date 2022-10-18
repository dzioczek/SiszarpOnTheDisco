using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SiszarpOnTheDisco.Models.MusicLinks;
using SiszarpOnTheDisco.Plugins;

namespace SiszarpOnTheDisco.CommandModules;

public class MusicCommands : ModuleBase<SocketCommandContext>
{
    private readonly ulong _musicChannelId;
    private readonly MusicLinksPlugin _musicLinksPlugin;

    public MusicCommands(MusicLinksPlugin musicLinksPlugin)
    {
        _musicLinksPlugin = musicLinksPlugin;
        _musicChannelId = ulong.Parse(Environment.GetEnvironmentVariable("MUSIC_CHANNEL_ID"));
    }

    [Command("dajseta", RunMode = RunMode.Async)]
    [Summary("Losuje seta z bazy.")]
    public async Task GetRandomLink()
    {
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine($"Message time: {Context.Message.Timestamp.ToString()}");

        stringBuilder.AppendLine($"before getting link: {DateTimeOffset.Now.ToString()}");
        MusicLink musicLink = _musicLinksPlugin.GetRandomLink();
        stringBuilder.AppendLine($"after getting link: {DateTimeOffset.Now.ToString()}");
        stringBuilder.AppendLine(musicLink.Stats);
        EmbedBuilder builder = new()
        {
            Title = musicLink.url,
            Url = musicLink.url,
            Description = stringBuilder.ToString(),
            Color = Color.Red
        };
        await Context.Channel.SendMessageAsync(embed: builder.Build());
    }

    [Command("dejkrisa")]
    [Summary("No chyba wiadomo...")]
    public async Task Manieczki()
    {
        await ReplyAsync("A masz se posłuchaj... https://www.youtube.com/watch?v=N1KbTCS-Sz8:");
    }

    [Command("dodajseta", RunMode = RunMode.Async)]
    [Summary("Dodaje seta do bazy. Podaj link do seta.")]
    public async Task SaveSet(string link)
    {
        string response = _musicLinksPlugin.AddNewLink(link);
        await ReplyAsync(response);

        if (!response.Contains("Było!") & !Context.Channel.Id.Equals(_musicChannelId))
        {
            IGuild guild = Context.Guild;
            ITextChannel textChannel = await guild.GetTextChannelAsync(_musicChannelId);

            await textChannel.SendMessageAsync($"{Context.User.Username} dodał: {response} link: {link}");
        }
    }

    public async Task test()
    {
        IGuild guild = Context.Guild;
        IVoiceChannel voiceChannel = await guild.GetVoiceChannelAsync(123);
    }

    [Command("szukaj", RunMode = RunMode.Async)]
    [Alias("szukajseta")]
    [Summary("Szuka seta w bazie. Słowa kluczowe rozdzielone spacjami. Szuka w tytułach i tagach.")]
    public async Task FindSet([Remainder] string args)
    {
        EmbedBuilder builder = new()
        {
            Description = _musicLinksPlugin.SearchSet(args),
            Color = Color.Red
        };

        await Context.Channel.SendMessageAsync(embed: builder.Build());
    }


    [Command("yay", RunMode = RunMode.Async)]
    [Summary("Głosuj na tak!")]
    public async Task VoteYay()
    {
        bool result = await Task.FromResult(_musicLinksPlugin.Vote(true));

        if (result) await ReplyAsync("Cieszę się, że się podobało!");
        else await ReplyAsync("Nie było nic do głosowania.");
    }

    [Command("meh", RunMode = RunMode.Async)]
    [Summary("Głosuj na nie :(")]
    public async Task VoteMeh()
    {
        bool result = await Task.FromResult(_musicLinksPlugin.Vote(false));

        if (result) await ReplyAsync("Może następnym razem...");
        else await ReplyAsync("Nie było nic do głosowania.");
    }

    [Command("tagujseta", RunMode = RunMode.Async)]
    [Summary("Tagowanie seta. Podaj link i tagi rozdzielone spacjami.")]
    public async Task TagSet([Remainder] string input)
    {
        await ReplyAsync(_musicLinksPlugin.TagSet(input));
    }

    [Command("usunseta", RunMode = RunMode.Async)]
    [Alias("wypierdolseta")]
    [Summary("Usuwa seta z bazy.")]
    public async Task DeleteSet([Remainder] string link)
    {
        await ReplyAsync(_musicLinksPlugin.DeleteSet(link).Result);
    }
}