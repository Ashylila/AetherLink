using System;
using System.Numerics;
using AetherLink.DalamudServices;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;

namespace AetherLink.Windows;

public class ConfigWindow : Window, IDisposable
{
    private IPluginLog Log => Svc.Log;
    private Configuration Configuration;
    private string discordUserId = string.Empty;
    private string discordBotToken = string.Empty;
    public ConfigWindow(Plugin plugin) : base("AetherLink Config###ID")
    {
        Flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(450, 115);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
       
    }

    public override void Draw()
    {
        
        ImGui.Text("Discord Bot Token:   ");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(300);
        ImGui.InputText("##botToken", ref discordBotToken, 80);
        ImGui.Text("Your Discord userId:"); ImGui.SameLine();
        ImGui.SetNextItemWidth(300);
        ImGui.InputText("##DiscordUserID", ref discordUserId, 20);
        if (ImGui.Button("Save"))
        {
            Configuration.DiscordUserId = ulong.Parse(discordUserId);
            Log.Debug($"DiscordUserId: {Configuration.DiscordUserId}");
            Configuration.DiscordToken = discordBotToken;
            Configuration.Save();
            discordBotToken = string.Empty;
            discordUserId = string.Empty;
        }
    }
}
