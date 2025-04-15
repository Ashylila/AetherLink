using System.Threading.Tasks;
using AetherLink.Models;
using Discord.WebSocket;
using System.Linq;
using AetherLink.Utility;
using Discord;
using System.Collections.Generic;
using AetherLink.DalamudServices;
using AetherLink;
using Discord.Interactions;

namespace AetherLink.Discord.SlashCommands;

public class CurrentFlagsCommand : InteractionModuleBase<SocketInteractionContext>
{
    private Configuration _configuration;
    
    public CurrentFlagsCommand(Configuration config)
    {
        _configuration = config;
    }

    public async Task Execute()
    {
            
            var flags = string.Join("\n", _configuration.ChatTypes.Select(flag => $"• {flag}"));
            var embed = new EmbedBuilder()
                .WithTitle("Here are the current active flags:")
                .WithDescription(flags)
                .WithColor(Color.Blue)
                .Build();
            await RespondAsync(embed: embed);
        
    }
}
