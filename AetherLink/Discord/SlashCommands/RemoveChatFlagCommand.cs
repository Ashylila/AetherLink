using System.Threading.Tasks;
using AetherLink.Models;
using Discord.WebSocket;
using System.Linq;
using AetherLink.Utility;
using AetherLink;
using Dalamud.Game.Text;
using AetherLink.DalamudServices;
using Discord;
using System.Collections.Generic;
using Discord.Interactions;

namespace AetherLink.Discord.SlashCommands;
public class RemoveChatFlagCommand : InteractionModuleBase<SocketInteractionContext>
{
    private Configuration _config;
    
    public RemoveChatFlagCommand(Configuration config)
    {
        _config = config;
    }
    [SlashCommand("removechatflag", "Remove a chat flag.")]
    public async Task Execute([Summary("flag", "the flag to remove")]string flagToRemove)
    {
            if (!EnumHelper.IsValidEnumMember<XivChatType>(flagToRemove) || (EnumHelper.TryConvertToEnum<XivChatType>(flagToRemove, out var result) && !_config.ChatTypes.Contains(result)))
            {
                await RespondAsync("Invalid flag or it is already inactive.", ephemeral: true);
                await Task.Delay(5000);
                await DeleteOriginalResponseAsync();
                return;
            }
            _config.ChatTypes.Remove(result);
            _config.Save();
            await RespondAsync($"Flag {flagToRemove} has been removed", ephemeral: true);
            await Task.Delay(5000);
            await DeleteOriginalResponseAsync();
    }
}
