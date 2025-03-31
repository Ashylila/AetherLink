using System.Threading.Tasks;
using AetherLink.Models;
using Discord.WebSocket;
using System.Linq;
using AetherLink.Utility;
using Discord;
using System.Collections.Generic;
using AetherLink.DalamudServices;
using AetherLink;

namespace AetherLink.Discord.SlashCommands;

public class CurrentFlagsCommand : ICommand
{
    public string Name => "currentflags";
    public string Description => "View the current chat flags.";
    public List<CommandOption> Options { get; } = new();

    public async Task Execute(SocketInteraction interaction)
    {
        if (interaction is SocketSlashCommand command)
        {
            var config = Plugin.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            var flags = string.Join("\n", config.ChatTypes.Select(flag => $"• {flag}"));
            var embed = new EmbedBuilder()
                .WithTitle("Here are the current active flags:")
                .WithDescription(flags)
                .WithColor(Color.Blue)
                .Build();
            await interaction.RespondAsync(embed: embed);
            return;
        }
    }
}
