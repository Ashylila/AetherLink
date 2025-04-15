using System;
using System.Threading.Tasks;
using AetherLink.Models;
using Discord.WebSocket;
using System.Linq;
using AetherLink.Utility;
using Dalamud.Game.Text;
using AetherLink.DalamudServices;
using AetherLink;
using Discord;
using System.Collections.Generic;
using Discord.Interactions;

namespace AetherLink.Discord.SlashCommands;
public class AddChatFlagCommand : InteractionModuleBase<SocketInteractionContext>
{
    private Configuration _configuration;

    public AddChatFlagCommand(Configuration config)
    {
        _configuration = config;
    }
    [SlashCommand("addchatflag", "Add a chat flag.")]
    public async Task Execute([Summary("flag", "The chat flag to add.")] string flag)
    {
        
            
            if (!EnumHelper.IsValidEnumMember<XivChatType>(flag) || (EnumHelper.TryConvertToEnum<XivChatType>(flag, out var addresult) && _configuration.ChatTypes.Contains(addresult)))
            {
                await RespondAsync("Invalid flag or it is already active.", ephemeral: true);
                await Task.Delay(5000);
                await DeleteOriginalResponseAsync();
                return;
            }
            _configuration.ChatTypes.Add(addresult);
            _configuration.Save();
            await RespondAsync($"Flag {flag} has been added", ephemeral: true);
            await Task.Delay(5000);
            await DeleteOriginalResponseAsync();
    }
}
