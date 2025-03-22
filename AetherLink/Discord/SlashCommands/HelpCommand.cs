using System.Threading.Tasks;
using AetherLink.Models;
using Discord.WebSocket;
using System.Linq;
using AetherLink.Utility;
using Discord;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace AetherLink.Discord.SlashCommands;

public class HelpCommand : ICommand
{
    public string Name => "help";
    public string Description => "Show current commands.";
    public List<CommandOption> Options { get; } = new();

    public async Task Execute(SocketInteraction interaction)
    {
        if (interaction is SocketSlashCommand command)
        {
            var commandTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            var embed = new EmbedBuilder()
                .WithTitle("Here are the current commands:")
                .WithColor(Color.Blue);
            foreach (var type in commandTypes)
            {
                if (Activator.CreateInstance(type) is ICommand cmd)
                {
                    embed.AddField(cmd.Name, cmd.Description);
                }
            }
            await interaction.RespondAsync(embed: embed.Build());
        }
    }
}