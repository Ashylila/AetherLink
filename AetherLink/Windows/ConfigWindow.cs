using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace AetherLink.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;
    private string discordUserId;
    private string discordBotToken;
    public ConfigWindow(Plugin plugin) : base("A Wonderful Configuration Window###With a constant ID")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(232, 90);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        // Flags must be added or removed before Draw() is being called, or they won't apply
        if (Configuration.IsConfigWindowMovable)
        {
            Flags &= ~ImGuiWindowFlags.NoMove;
        }
        else
        {
            Flags |= ImGuiWindowFlags.NoMove;
        }
    }

    public override void Draw()
    {
        var movable = Configuration.IsConfigWindowMovable;
        if (ImGui.Checkbox("Movable Config Window", ref movable))
        {
            Configuration.IsConfigWindowMovable = movable;
            Configuration.Save();
        }
        ImGui.Text("Discord Bot Token");
        ImGui.SameLine();
        ImGui.InputText("##botToken", ref discordBotToken, 500);

        if (ImGui.Button("Save"))
        {
            Configuration.DiscordToken = discordBotToken;
            Configuration.Save();
            discordBotToken = string.Empty;

        }

        ImGui.Text("Your Discord userId:"); ImGui.SameLine();
        ImGui.InputText("##DiscordUserID", ref discordUserId, 100);
        if (ImGui.Button("Save"))
        {
            Configuration.DiscordUserId = ulong.Parse(discordUserId);
            Configuration.Save();
            discordUserId = string.Empty;
        }
    }
}
