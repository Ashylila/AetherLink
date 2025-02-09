using System;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina.Excel.Sheets;

namespace AetherLink.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;
    private Configuration configuration;
    public MainWindow(Plugin plugin)
        : base("AetherLink##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(100, 50),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        Plugin = plugin;
        configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {

        if (ImGui.Button("Show Settings"))
        {
            Plugin.ToggleConfigUI();
        }

        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.TextUnformatted("Bot Status: " + Plugin.DiscordHandler.isConnected.ToString());

    }
}
