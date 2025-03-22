using System.Threading.Tasks;
using AetherLink.Models;
using Discord.WebSocket;
using System.Linq;
using AetherLink.Utility;
using System.Collections.Generic;
using Discord;

namespace AetherLink.Discord.SlashCommands;
public class SendMessageCommand : ICommand
{
    public string Name => "sendmessage";
    public string Description => "Send a message to the chat.";
    public List<CommandOption> Options { get; } = new()
    {
        new CommandOption()
        {
            Name = "message",
            Description = "The message to send",
            Type = ApplicationCommandOptionType.String,
            IsRequired = true,
        },
    };

    public async Task Execute(SocketInteraction interaction)
    {
        if (interaction is SocketSlashCommand command)
        {
            var chatmessage = command.Data.Options.FirstOrDefault(x => x.Name == "message")?.Value as string;
            ChatMessageSender.SendChatMessage(chatmessage);
            await interaction.RespondAsync($"Message has been sent to the chat: {chatmessage}", ephemeral: true);
            await Task.Delay(5000);
            await interaction.DeleteOriginalResponseAsync();
            return;
        }
    }
}