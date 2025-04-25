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
using AetherLink.Utility;
using Discord.Interactions;

namespace AetherLink.Discord;

public class CommandHandler : IDisposable
{
    private readonly IPluginLog _log;
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly IChatGui _chatGui;
    private IFramework Framework => Svc.Framework;

    public CommandHandler(DiscordSocketClient client, InteractionService interactionService, IPluginLog Log, IChatGui chatGui)
    {
        _chatGui = chatGui;
        _log = Log;
        _client = client;
        _interactionService = interactionService;
    }
    
    public async Task InitializeAsync()
    {
        var assembly = Assembly.GetExecutingAssembly();

        if (_interactionService == null)
        {
            _log.Error("InteractionService is null!");
            return;
        }

        if (ServiceWrapper.Services == null)
        {
            _log.Error("ServiceProvider is null!");
            return;
        }

        await _interactionService.AddModulesAsync(assembly, ServiceWrapper.Services);
        try
        {
            await _interactionService.RegisterCommandsGloballyAsync();
            _log.Information("Interaction modules loaded.");
        }catch (Exception ex)
        {
            _log.Error(ex, "Failed to register commands globally.");
            _chatGui.PrintError("Failed to register commands globally, you can try to restart the process in the config window.", messageTag: "AetherLink", tagColor: 51447);
        }

        _client.InteractionCreated += HandleInteraction;
        
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(_client, interaction);
            if (interaction is SocketSlashCommand slashCommand)
            {
                _log.Debug($"[{DateTime.Now.TimeOfDay}] {interaction.User} has executed the command {slashCommand.CommandName}");
            }
            var result = await _interactionService.ExecuteCommandAsync(context, ServiceWrapper.Services);
            if (!result.IsSuccess)
            {
                await interaction.RespondAsync(result.ErrorReason);
            }
        }catch (Exception ex)
        {
            _log.Error($"Exception while handling interaction: {ex.Message}");
        }
    }
    public void Dispose()
    {
        _client.InteractionCreated -= HandleInteraction;
    }
}
