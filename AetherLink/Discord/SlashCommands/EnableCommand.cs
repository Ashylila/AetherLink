using System.Threading.Tasks;
using AetherLink.Models;
using Discord.WebSocket;
using System.Linq;
using AetherLink.Utility;
using Discord;
using System.Collections.Generic;
using System.Reflection;
using System;
using AetherLink.DalamudServices;

namespace AetherLink.Discord.SlashCommands;

public class EnableCommand : ICommand
{
    public string Name => "enable";
    public string Description => "Enable the bot.";
    public List<CommandOption> Options { get; } = new();

    public async Task Execute(SocketInteraction interaction)
    {
        if (interaction is SocketSlashCommand command)
        {
            var configuration = Svc.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            if (configuration.IsChatLogEnabled)
            {
                await interaction.RespondAsync("The logging of the chat is already enabled", ephemeral: true);
                await Task.Delay(5000);
                await interaction.DeleteOriginalResponseAsync();
                return;
            }
            configuration.IsChatLogEnabled = true;
            configuration.Save();
            await interaction.RespondAsync("The logging of the chat has been enabled", ephemeral: true);
            await Task.Delay(5000);
            await interaction.DeleteOriginalResponseAsync();
            return;
        }
    }
}