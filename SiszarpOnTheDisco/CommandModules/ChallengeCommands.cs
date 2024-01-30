// Ignore Spelling: Siszarp

using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiszarpOnTheDisco.CommandModules
{
    public class ChallengeCommands : InteractionModuleBase
    {
        public ChallengeCommands() { }

        [SlashCommand("cstatus", "status", runMode: RunMode.Async)]
        public async Task ChallengeStatus()
        {
            await RespondAsync("test");
        }
    }
}
