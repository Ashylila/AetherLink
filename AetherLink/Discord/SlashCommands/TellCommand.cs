using System.Threading.Tasks;
using AetherLink.Models;
using Discord.WebSocket;
using System.Linq;
using AetherLink.Utility;
using System.Collections.Generic;
using Discord;

namespace AetherLink.Discord.SlashCommands;
public class TellCommand : ICommand
{
    public string Name => "tell";
    public string Description => "Send a tell to a player.";
    public List<CommandOption> Options { get; } = new()
    {
        new CommandOption()
        {
            Name = "target",
            Description = "The player to send the tell to",
            Type = ApplicationCommandOptionType.String,
            IsRequired = true,
            IsAutoFill = true
        },
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
            var target = command.Data.Options.FirstOrDefault(x => x.Name == "target")?.Value as string;
            var message = command.Data.Options.FirstOrDefault(x => x.Name == "message")?.Value as string;
            ChatMessageSender.SendTellMessage(target, message);
            await interaction.RespondAsync($"responded to:{target} with {message}", ephemeral: true);
            await Task.Delay(5000);
            await interaction.DeleteOriginalResponseAsync();
        }
    }
}