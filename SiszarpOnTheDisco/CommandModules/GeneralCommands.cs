using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Sprocket.Text.Ascii;

namespace SiszarpOnTheDisco.CommandModules;

public class GeneralCommands : ModuleBase<SocketCommandContext>
{
    private readonly CommandService _commandsService;

    public GeneralCommands(CommandService commandService)
    {
        _commandsService = commandService;
    }

    [Command("help")]
    public async Task GetHelp()
    {
        StringBuilder stringBuilder = new();

        stringBuilder.AppendLine("Displaying list of available modules.");
        stringBuilder.AppendLine("To get details on certain module type: !help [moduleName]");
        stringBuilder.AppendLine(string.Empty);

        foreach (ModuleInfo moduleInfo in _commandsService.Modules)
        {
            stringBuilder.AppendLine($"{moduleInfo.Name.Replace("Commands", string.Empty)}");
        }

        EmbedBuilder builder = new()
        {
            Title = "help",
            Description = stringBuilder.ToString(),
            Color = Color.Blue
        }; 

        await ReplyAsync(embed: builder.Build());
    }

    [Command("help")]
    public async Task GetHelp([Remainder] string moduleName)
    {
        ModuleInfo moduleInfo = _commandsService.Modules
            .FirstOrDefault(x => string.Equals(x.Name.Replace("Commands", string.Empty), moduleName,
                StringComparison.CurrentCultureIgnoreCase));

        if (moduleInfo == null)
        {
            await ReplyAsync("Module not found.");
            return;
        }

        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine($"List of commands for module {moduleName}");

        foreach (CommandInfo command in moduleInfo.Commands)
        {
            StringBuilder paramStringBuilder = new(); 
            if (command.Parameters.Count > 0)
            {
                foreach (ParameterInfo commandParameter in command.Parameters)
                {
                    paramStringBuilder.Append($"[{commandParameter.Summary}] ");
                }
            }
            stringBuilder.AppendLine($"{command.Name} {(paramStringBuilder.Length > 0 ? paramStringBuilder.ToString() : string.Empty)} - {command.Summary}");
        }

        EmbedBuilder builder = new()
        {
            Title = $"help {moduleName}",
            Description = stringBuilder.ToString(),
            Color = Color.Blue
        };

        await ReplyAsync(embed: builder.Build());
    }

    [Command("date")]
    public async Task GetDate()
    {
        await ReplyAsync(DateTime.Today.ToLongDateString());
    }

    [Command("cal")]
    public async Task GetCal()
    {
        Calendar c = new()
        {
            CellHeight = 1,
            CellWidth = 5,
            RenderWeekRowSeparators = false
        };
        
        await ReplyAsync($"```{c.Render(DateTime.Now)}```");
    }
}