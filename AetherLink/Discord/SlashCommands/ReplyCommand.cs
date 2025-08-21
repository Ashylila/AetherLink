using System.Threading.Tasks;
using AetherLink.Models;
using Discord.WebSocket;
using System.Linq;
using AetherLink.Utility;
using System.Collections.Generic;
using Discord;
using Discord.Interactions;

namespace AetherLink.Discord.SlashCommands;
public class ReplyCommand : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("reply", "Send a message to the last person who messaged you")]
    public async Task Execute([Summary("message", "the message to send")] string replymessage)
    {
        var success = ChatMessageSender.SendChatMessage("/r " + replymessage);
        if (success)
            await RespondAsync($"Responded with: {replymessage}", ephemeral: true);
        else
            await RespondAsync("❌ Failed to send reply.", ephemeral: true);
    }
}
