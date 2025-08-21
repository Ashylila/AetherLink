using System.Threading.Tasks;
using AetherLink.Models;
using Discord.WebSocket;
using System.Linq;
using AetherLink.Utility;
using Discord;
using System.Collections.Generic;
using Discord.Interactions;

namespace AetherLink.Discord.SlashCommands;

public class FcCommand : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("fc", "Send a message to the Free Company")]
    public async Task Execute([Summary("message", "the message to send")] string fcmessage)
    {
        var success = ChatMessageSender.SendFreeCompanyMessage(fcmessage);
        if (success)
            await RespondAsync($"Message has been sent to the Free Company: {fcmessage}", ephemeral: true);
        else
            await RespondAsync("❌ Failed to send Free Company message.", ephemeral: true);
    }
}
