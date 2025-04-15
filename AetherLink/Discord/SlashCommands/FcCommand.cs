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
            ChatMessageSender.SendFreeCompanyMessage(fcmessage);
            await RespondAsync($"Message has been sent to the Free Company: {fcmessage}", ephemeral: true);
            await Task.Delay(5000);
            await DeleteOriginalResponseAsync();
    }
}
