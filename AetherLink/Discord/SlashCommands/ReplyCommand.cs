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
            ChatMessageSender.SendChatMessage("/r " + replymessage);
            await RespondAsync($"responded with {replymessage}", ephemeral: true);
            await Task.Delay(5000);
            await DeleteOriginalResponseAsync();
    }
}
