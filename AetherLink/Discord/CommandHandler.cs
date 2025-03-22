using AetherLink.Models;
using Dalamud.Plugin.Services;
using AetherLink.DalamudServices;
using System.Collections.Generic;
using Discord.WebSocket;
using System.Reflection;
using System.Linq;
using System;
using System.Threading.Tasks;
using Discord;
using System.Linq.Expressions;

namespace AetherLink.Discord;

public class CommandHandler
{
    private IPluginLog Logger => Svc.Log;
    private readonly Dictionary<string, ICommand> _commands = new();
    private readonly DiscordSocketClient client;

    public CommandHandler(DiscordSocketClient client)
    {
        this.client = client;
        LoadCommands();
        client.InteractionCreated += HandleCommand;
        client.Ready += async () =>
        {
            await RegisterCommands();
        };
    }

    private void LoadCommands()
    {
        var commandTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
        foreach (var type in commandTypes)
        {
            if (Activator.CreateInstance(type) is ICommand command)
            {
                _commands[command.Name] = command;
            }
        }
    }
    private async Task RegisterCommands()
    {
        try
        {
            var currentCommands = await client.GetGlobalApplicationCommandsAsync();
            var newCommands = _commands.Values.Where(command =>
            !currentCommands.Any(c => c.Name == command.Name)).ToList();

            if (!newCommands.Any())
            {
                Logger.Info("No new commands to register.");
                return;
            }
            foreach (var command in newCommands)
            {
                SlashCommandBuilder commandBuilder = new SlashCommandBuilder()
                    .WithName(command.Name)
                    .WithDescription(command.Description);
                foreach (var option in command.Options.Where(opt => !string.IsNullOrEmpty(opt.Name) ))
                {
                    commandBuilder.AddOption(name: option.Name, type: option.Type, description: option.Description, isRequired: option.IsRequired, isAutocomplete: option.IsAutoFill);
                }
                await client.Rest.CreateGlobalCommand(commandBuilder.Build());
                Logger.Debug($"Command {command.Name} registered");
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to register commands");
        }
    }
    private async Task HandleCommand(SocketInteraction interaction)
    {
        if (interaction is SocketSlashCommand command)
        {
            if (_commands.TryGetValue(command.Data.Name, out var cmd))
            {
                await cmd.Execute(interaction);
            }
            else
            {
                Logger.Warning($"Command {command.Data.Name} not found");
                await interaction.RespondAsync("Command not found", ephemeral: true);
            }
        }
    }
}