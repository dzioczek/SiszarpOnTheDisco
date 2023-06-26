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
    public async Task GetHelp([Summary("nazwa", "Nazwa modułu.")]string moduleName = "")
    {
        if (string.IsNullOrEmpty(moduleName))
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Displaying list of available modules.");
            sb.AppendLine("To get details on certain module type: /help [moduleName]");
            sb.AppendLine(string.Empty);

            foreach (ModuleInfo module in _interactionService.Modules)
            {
                sb.AppendLine($"{module.Name.Replace("Commands", string.Empty)}");
            }

            EmbedBuilder builder = new()
            {
                Title = "help",
                Description = sb.ToString(),
                Color = Color.Blue
            };

            await RespondAsync(embed: builder.Build());
        }
        else
        {
            ModuleInfo moduleInfo = _interactionService.Modules
       .FirstOrDefault(x => string.Equals(x.Name.Replace("Commands", string.Empty), moduleName,
           StringComparison.CurrentCultureIgnoreCase));

            if (moduleInfo == null)
            {
                await ReplyAsync("Module not found.");
                return;
            }

            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine($"List of commands for module {moduleName}");

            foreach (SlashCommandInfo command in moduleInfo.SlashCommands)
            {
                StringBuilder paramStringBuilder = new();
                if (command.Parameters.Count > 0)
                {
                    foreach (SlashCommandParameterInfo commandParameter in command.Parameters)
                    {
                        paramStringBuilder.Append($"[{commandParameter.Name}]");
                    }
                }
                stringBuilder.AppendLine($"{command.Name} {(paramStringBuilder.Length > 0 ? paramStringBuilder.ToString() : string.Empty)} - {command.Description}");
            }

            EmbedBuilder builder = new()
            {
                Title = $"help {moduleName}",
                Description = stringBuilder.ToString(),
                Color = Color.Blue
            };

            await RespondAsync(embed: builder.Build());
        }
    }

}

