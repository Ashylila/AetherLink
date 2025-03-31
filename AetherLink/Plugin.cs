using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using AetherLink.Windows;
using AetherLink.DalamudServices;
using AetherLink.Discord;
using Discord.Rest;

namespace AetherLink;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    internal static IPluginLog Log  => Svc.Log;

    private const string CommandName = "/aetherlink";

    public DiscordHandler DiscordHandler;
    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("AetherLink");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    private LogWindow LogWindow { get; init; }

    public Plugin()
    {
        Svc.Init(PluginInterface);
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        DiscordHandler = new DiscordHandler(this);

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);
        LogWindow = new LogWindow(this);
        
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

        Init();
    }

    private async void Init()
    {
        await DiscordHandler._init();
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();
        DiscordHandler.Dispose();
        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        Log.Verbose(args);
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
