using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SiszarpOnTheDisco.Plugins;

namespace SiszarpOnTheDisco.CommandModules;

public class AllergenCommands : ModuleBase<SocketCommandContext>
{
    private readonly AllergensPlugin _allergensPlugin;

    public AllergenCommands(AllergensPlugin allergensPlugin)
    {
        _allergensPlugin = allergensPlugin;
    }

    [Command("copyli")]
    public async Task GetDusts()
    {
        EmbedBuilder builder = new()
        {
            Description = _allergensPlugin.GetAllAllergens().Result,
            Color = Color.DarkMagenta
        };


        await Context.Channel.SendMessageAsync(embed: builder.Build());
    }
    
    [Command("coznowukurwapyli")]
    [Alias("cokurwapyli", "alertpylenia")]
    public async Task GetAlert()
    {
        await Context.Channel.SendMessageAsync(_allergensPlugin.GetAlert().Result);
    }
}