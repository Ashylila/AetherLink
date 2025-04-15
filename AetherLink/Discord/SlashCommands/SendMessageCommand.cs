using System.Threading.Tasks;
using AetherLink.Models;
using Discord.WebSocket;
using System.Linq;
using AetherLink.Utility;
using System.Collections.Generic;
using Discord;
using Discord.Interactions;

namespace AetherLink.Discord.SlashCommands;
public class SendMessageCommand : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("send", "Send a message to the chat")]
    public async Task Execute([Summary("message", "the message to send")] string chatmessage)
    {
            ChatMessageSender.SendChatMessage(chatmessage);
            await RespondAsync($"Message has been sent to the chat: {chatmessage}", ephemeral: true);
            await Task.Delay(5000);
            await DeleteOriginalResponseAsync();
        }
    }
