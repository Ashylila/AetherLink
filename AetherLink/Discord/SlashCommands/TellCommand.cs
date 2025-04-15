using System.Threading.Tasks;
using AetherLink.Models;
using Discord.WebSocket;
using System.Linq;
using AetherLink.Utility;
using System.Collections.Generic;
using AetherLink.Discord.AutoFillHandlers;
using Discord;
using Discord.Interactions;

namespace AetherLink.Discord.SlashCommands;
public class TellCommand : InteractionModuleBase<SocketInteractionContext>
{
[SlashCommand("tell", "Send a message to another player.")]
    public async Task Execute([Summary("target", "the target to send the message to")] [Autocomplete(typeof(TellHandler))]string target, [Summary("message", "the message to send")] string message)
    {
            ChatMessageSender.SendTellMessage(target, message);
            await RespondAsync($"responded to:{target} with {message}", ephemeral: true);
            await Task.Delay(5000);
            await DeleteOriginalResponseAsync();
    }
}
