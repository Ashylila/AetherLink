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

public class EnableCommand : InteractionModuleBase<SocketInteractionContext>
{
    private Configuration _configuration;
    
    public EnableCommand(Configuration config)
    {
        _configuration = config;
    }
    [SlashCommand("enable", "Enable the logging of the chat.")]
    public async Task Execute()
    {
        
            if (_configuration.IsChatLogEnabled)
            {
                await RespondAsync("The logging of the chat is already enabled", ephemeral: true);
                await Task.Delay(5000);
                await DeleteOriginalResponseAsync();
                return;
            }
            _configuration.IsChatLogEnabled = true;
            _configuration.Save();
            await RespondAsync("The logging of the chat has been enabled", ephemeral: true);
            await Task.Delay(5000);
            await DeleteOriginalResponseAsync();
        
    }
}
