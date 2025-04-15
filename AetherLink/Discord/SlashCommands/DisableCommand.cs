using System.Threading.Tasks;
using AetherLink.Models;
using Discord.WebSocket;
using System.Linq;
using AetherLink.Utility;
using Discord;
using System.Collections.Generic;
using System.Reflection;
using System;
using AetherLink.DalamudServices;
using Discord.Interactions;

namespace AetherLink.Discord.SlashCommands;

public class DisableCommand : InteractionModuleBase<SocketInteractionContext>
{
    private Configuration _configuration;

    public DisableCommand(Configuration config)
    {
        _configuration = config;
    }
    [SlashCommand("disable", "Disable the logging of the chat.")]
    public async Task Execute()
    {
        
            
            if (!_configuration.IsChatLogEnabled)
            {
                await RespondAsync("The logging of the chat is already disabled", ephemeral: true);
                await Task.Delay(5000);
                await DeleteOriginalResponseAsync();
                return;
            }
            _configuration.IsChatLogEnabled = false;
            _configuration.Save();
            await RespondAsync("The logging of the chat has been disabled", ephemeral: true);
            await Task.Delay(5000);
            await DeleteOriginalResponseAsync();
            return;
        
    }
}
