using System.Threading.Tasks;
using AetherLink.Models;
using Discord.WebSocket;
using System.Linq;
using AetherLink.Utility;
using Discord;
using System.Collections.Generic;

namespace AetherLink.Discord.SlashCommands;

public class FcCommand : ICommand
{
    public string Name => "fc";
    public string Description => "Send a message to the Free Company.";
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
            var fcmessage = command.Data.Options.FirstOrDefault(x => x.Name == "message")?.Value as string;
            ChatMessageSender.SendFreeCompanyMessage(fcmessage);
            await interaction.RespondAsync($"Message has been sent to the Free Company: {fcmessage}", ephemeral: true);
            await Task.Delay(5000);
            await interaction.DeleteOriginalResponseAsync();
        }
    }
}