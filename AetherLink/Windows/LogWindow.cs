using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Utility;
using System.Collections.Generic;
using System.Text;
using Dalamud.Game.Text;



namespace AetherLink.Windows;

public class LogWindow : Window, IDisposable
{
    private string searchQuery = string.Empty;
    private XivChatType? selectedChatType = null;
    private Plugin Plugin;
    private Configuration configuration;
    public LogWindow(Plugin plugin)
        : base("Chatlog##With a hidden ID")
    {
        Size = new Vector2(1500, 800);
        Plugin = plugin;
        configuration = plugin.Configuration;
    }
    public override void Draw()
    {
        
        ImGui.BeginChild("FilterControls", new Vector2(0, 50), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        
        
        ImGui.Text("Filter by Chat Type:");
        ImGui.SameLine();
        string selectedChatTypeName = selectedChatType?.ToString() ?? "Select Chat Type";
        if (ImGui.BeginCombo("##chatType", selectedChatTypeName))
        {
            foreach (var chatType in Enum.GetValues(typeof(XivChatType))) 
            {
                if (ImGui.Selectable(chatType.ToString()))
                {
                    selectedChatType = (XivChatType)chatType;
                }
            }
            ImGui.EndCombo();
        }

        
        ImGui.Text("Search:");
        ImGui.SameLine();
        ImGui.InputText("##searchBar", ref searchQuery, 100); 

        ImGui.EndChild();

        
        ImGui.BeginChild("LogContent", new Vector2(0, 0), true);

        StringBuilder filteredLogContent = new StringBuilder();
        foreach (var line in Plugin.Configuration.ChatLog.ToString().Split('\n'))
        {
            if ((selectedChatType == null || line.Contains($"[{selectedChatType.ToString()}]")) &&
            (string.IsNullOrEmpty(searchQuery) || line.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)))
            {
            filteredLogContent.AppendLine(line);
            }
        }

        foreach (var line in filteredLogContent.ToString().Split('\n'))
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
            ImGui.Separator();
            var parts = line.Split(new[] { ']' }, 3);
            if (parts.Length == 3)
            {
                ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1.0f), parts[0] + "]"); 
                ImGui.SameLine();
                ImGui.TextColored(GetColorForType(parts[1].Trim('[', ']')), parts[1] + "]"); 
                ImGui.SameLine();
                var restParts = parts[2].Split(new[] { ':' }, 2); 
                if (restParts.Length == 2)
                {
                ImGui.TextColored(new Vector4(0.0f, 0.5f, 1.0f, 1.0f), restParts[0] + ":"); 
                ImGui.SameLine();
                ImGui.TextWrapped(restParts[1]); 
                }
                else
                {
                ImGui.TextWrapped(parts[2]); 
                }
            }
            else
            {
                ImGui.TextWrapped(line); 
            }
            }
        }

        ImGui.EndChild();

        Vector4 GetColorForType(string chatType)
        {
            return chatType switch
            {
            "FreeCompany" => new Vector4(0.0f, 0.0f, 1.0f, 1.0f), // Blue
            "Ls1" => new Vector4(0.0f, 1.0f, 0.0f, 1.0f), // Green
            "Ls2" => new Vector4(0.0f, 1.0f, 0.0f, 1.0f), // Green
            "Ls3" => new Vector4(0.0f, 1.0f, 0.0f, 1.0f), // Green
            "Ls4" => new Vector4(0.0f, 1.0f, 0.0f, 1.0f), // Green
            "Ls5" => new Vector4(0.0f, 1.0f, 0.0f, 1.0f), // Green
            "Ls6" => new Vector4(0.0f, 1.0f, 0.0f, 1.0f), // Green
            "Ls7" => new Vector4(0.0f, 1.0f, 0.0f, 1.0f), // Green
            "Ls8" => new Vector4(0.0f, 1.0f, 0.0f, 1.0f), // Green
            "CrossLinkShell1" => new Vector4(0.5f, 0.0f, 0.5f, 1.0f), // Purple
            "CrossLinkShell2" => new Vector4(0.5f, 0.0f, 0.5f, 1.0f), // Purple
            "CrossLinkShell3" => new Vector4(0.5f, 0.0f, 0.5f, 1.0f), // Purple
            "TellIncoming" => new Vector4(1.0f, 0.0f, 1.0f, 1.0f), // Magenta
            "TellOutgoing" => new Vector4(1.0f, 0.0f, 1.0f, 1.0f), // Magenta
            "Say" => new Vector4(0.75f, 0.75f, 0.75f, 1.0f), // LighterGrey
            "Shout" => new Vector4(1.0f, 0.5f, 0.0f, 1.0f), // Orange
            "Party" => new Vector4(0.0f, 0.0f, 1.0f, 1.0f), // Blue
            _ => new Vector4(0.75f, 0.75f, 0.75f, 1.0f), // LightGrey
            };
        }
    }
    public void Dispose() 
    {

    }
}