using System.Threading.Tasks;
using AetherLink.Models;
using Discord.WebSocket;
using System.Linq;
using AetherLink.Utility;
using Discord;
using System.Collections.Generic;
using Discord.Interactions;

namespace AetherLink.Discord.SlashCommands;
public class SayCommand : InteractionModuleBase<SocketInteractionContext>
{
[SlashCommand("say", "Send a message to the say chat")]
    public async Task Execute([Summary("message", "the message to send")] string saymessage)
    {
        if (string.IsNullOrWhiteSpace(saymessage))
        {
            await RespondAsync("Message cannot be empty.", ephemeral: true);
            return;
        }

        var success = ChatMessageSender.SendSayMessage(saymessage);

        if (success)
            await RespondAsync($"Message has been sent to /say: {saymessage}", ephemeral: true);
        else
            await RespondAsync("❌ Failed to send the message to /say.", ephemeral: true);
        
    }
}
