using System;
using System.Numerics;
using AetherLink.Discord;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Discord.WebSocket;
using ImGuiNET;
using Lumina.Excel.Sheets;

namespace AetherLink.Windows;

public class MainWindow : Window, IDisposable
{
    private DiscordHandler _discordClient;
    private Configuration configuration;
    private readonly Plugin plugin;
    public MainWindow(DiscordHandler client, Configuration config, Plugin plugin)
        : base("AetherLink##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        _discordClient = client;
        Size = new Vector2(150, 100);
        configuration = config;
        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {

        if (ImGui.Button("Show Settings"))
        {
            plugin.ToggleConfigUI();
        }

        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.TextUnformatted("Bot status:  ");
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, _discordClient.isConnected ? new Vector4(0, 1, 0, 1) : new Vector4(1, 0, 0, 1));
        ImGui.TextUnformatted(_discordClient.isConnected ? "Running" : "Stopped");
        ImGui.PopStyleColor();

    }
}
