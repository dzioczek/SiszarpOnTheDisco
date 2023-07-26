using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiszarpOnTheDisco.CommandModules;

public class HelpCommands : InteractionModuleBase
{
    private InteractionService _interactionService;
    public HelpCommands(InteractionService interactionService)
    {
        _interactionService = interactionService;
    }

    [SlashCommand("pomocy", "Get help.", runMode: RunMode.Async)]
    public async Task GetHelp([Summary("nazwa", "Nazwa modułu.")] string moduleName = "")
    {
        StringBuilder sb = new StringBuilder();
        if (string.IsNullOrEmpty(moduleName))
        {
            sb.AppendLine("Displaying list of available modules.");
            sb.AppendLine("To get details on certain module type: /help [moduleName]");
            sb.AppendLine(string.Empty);

            foreach (ModuleInfo module in _interactionService.Modules)
            {
                sb.AppendLine($"{module.Name.Replace("Commands", string.Empty)}");
            }
        }
        else
        {
            ModuleInfo moduleInfo = _interactionService.Modules
                                    .FirstOrDefault(x => string.Equals(x.Name.Replace("Commands", string.Empty), moduleName,
                                        StringComparison.CurrentCultureIgnoreCase));

            if (moduleInfo == null)
            {
                await RespondAsync("Module not found.");
                return;
            }

            sb.AppendLine($"List of commands for module {moduleName}");

            foreach (SlashCommandInfo command in moduleInfo.SlashCommands)
            {
                StringBuilder paramsSb = new();
                if (command.Parameters.Count > 0)
                {
                    foreach (SlashCommandParameterInfo commandParameter in command.Parameters)
                    {
                        paramsSb.Append($"[{commandParameter.Name}]");
                    }
                }
                sb.AppendLine($"{command.Name} {(paramsSb.Length > 0 ? paramsSb.ToString() : string.Empty)} - {command.Description}");
            }
        }

        EmbedBuilder builder = new()
        {
            Title = $"help {moduleName}",
            Description = sb.ToString(),
            Color = Color.Blue
        };

        await RespondAsync(embed: builder.Build());
    }
}

