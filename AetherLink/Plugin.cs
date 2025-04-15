using System;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Threading.Tasks;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using AetherLink.Windows;
using AetherLink.DalamudServices;
using AetherLink.Discord;
using AetherLink.Utility;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Serilog.Core;

namespace AetherLink;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;

    private const string CommandName = "/aetherlink";
    
    private IPluginLog Logger;
    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("AetherLink");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    private LogWindow LogWindow { get; init; }
    private DiscordSocketClient _client;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        ServiceWrapper.Init(PluginInterface, this);
        
        _client = ServiceWrapper.Get<DiscordSocketClient>();
            
        ConfigWindow = ServiceWrapper.Get<ConfigWindow>();
        MainWindow = ServiceWrapper.Get<MainWindow>();
        LogWindow = ServiceWrapper.Get<LogWindow>();
        
        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(LogWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open up the UI"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
        
        Logger = ServiceWrapper.Get<IPluginLog>();
        
        _client.Log += LogAsync;
        ServiceWrapper.Get<InteractionService>().Log += LogAsync;
        Init();
    }

    private async Task Init()
    {
        await ServiceWrapper.Get<DiscordHandler>()._init();
        _client.Ready += async () => await ServiceWrapper.Get<CommandHandler>().InitializeAsync();
        ServiceWrapper.Get<ChatHandler>().Init();
    }
    private Task LogAsync(LogMessage logMessage)
    {
        Logger.Debug(logMessage.Message);
        return Task.CompletedTask;
    }
    public void Dispose()
    {
        if (ServiceWrapper.Services is IDisposable disposable)
        {
            disposable.Dispose();
        }
        WindowSystem.RemoveAllWindows();
        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        switch (args.ToLower())
        {
            case "":
                ToggleMainUI();
                break;
            case "chatlog":
                ToggleChatLogUI();
                break;
        }
    }

    private void DrawUI() => WindowSystem.Draw();
    public void ToggleChatLogUI() => LogWindow.Toggle();
    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
