using System.Threading.Tasks;
using AetherLink.Models;
using Discord.WebSocket;
using System.Linq;
using AetherLink.Utility;
using Discord;
using System.Collections.Generic;

namespace AetherLink.Discord.SlashCommands;
public class SayCommand : ICommand
{
    public string Name => "say";
    public string Description => "Send a message to the say chat.";
    public List<CommandOption> Options {get;} = new()
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
        if(interaction is SocketSlashCommand command)
        {
            var saymessage = command.Data.Options.FirstOrDefault(x => x.Name == "message")?.Value as string;
            if(saymessage == null) return;
            ChatMessageSender.SendSayMessage(saymessage);
                            await interaction.RespondAsync($"Message has been sent to the say chat: {saymessage}", ephemeral: true);
                            await Task.Delay(5000);
                            await interaction.DeleteOriginalResponseAsync();
        }
    }
}