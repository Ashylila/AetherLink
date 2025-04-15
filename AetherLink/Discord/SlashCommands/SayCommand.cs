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
            if(saymessage == null) return;
            ChatMessageSender.SendSayMessage(saymessage);
            await RespondAsync($"Message has been sent to the say chat: {saymessage}", ephemeral: true);
            await Task.Delay(5000);
            await DeleteOriginalResponseAsync();
    }
}
