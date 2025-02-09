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
        Size = new Vector2(150, 100);
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
        ImGui.TextUnformatted("Bot status:  ");
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, Plugin.DiscordHandler.isConnected ? new Vector4(0, 1, 0, 1) : new Vector4(1, 0, 0, 1));
        ImGui.TextUnformatted(Plugin.DiscordHandler.isConnected ? "Running" : "Stopped");
        ImGui.PopStyleColor();

    }
}
