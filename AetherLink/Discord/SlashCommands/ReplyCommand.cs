using System.Threading.Tasks;
using AetherLink.Models;
using Discord.WebSocket;
using System.Linq;
using AetherLink.Utility;
using System.Collections.Generic;
using Discord;

namespace AetherLink.Discord.SlashCommands;
public class ReplyCommand : ICommand
{
    public string Name => "reply";
    public string Description => "Reply to a tell.";
    public List<CommandOption> Options { get; } = new()
    {
        new CommandOption()
        {
            Name = "message",
            Description = "The message to send",
            Type = ApplicationCommandOptionType.String,
            IsRequired = true
        },
    };

    public async Task Execute(SocketInteraction interaction)
    {
        if (interaction is SocketSlashCommand command)
        {
            var replymessage = command.Data.Options.FirstOrDefault(x => x.Name == "message")?.Value as string;
            ChatMessageSender.SendChatMessage("/r " + replymessage);
            await interaction.RespondAsync($"responded to:{command.Data.Options.FirstOrDefault(x => x.Name == "target")?.Value} with {replymessage}", ephemeral: true);
            await Task.Delay(5000);
            await interaction.DeleteOriginalResponseAsync();
            return;
        }
    }
}