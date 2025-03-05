using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Utility;
using System.Collections.Generic;
using System.Text;
using Dalamud.Game.Text;
using AetherLink.DalamudServices;
using System.IO;
using System.Text.Json;
using AetherLink.Models;


namespace AetherLink.Windows;

public class LogWindow : Window, IDisposable
{
    private List<ChatMessage> Chatlog = new List<ChatMessage>();
    private string searchQuery = string.Empty;
    private XivChatType? selectedChatType = null;
    private Plugin Plugin;
    private string FilePath;
    private Configuration configuration;
    public LogWindow(Plugin plugin)
        : base("Chatlog##With a hidden ID")
    {
        Size = new Vector2(1500, 800);
        Plugin = plugin;
        configuration = plugin.Configuration;
        FilePath = Path.Combine(Svc.PluginInterface.AssemblyLocation.Directory.FullName, "chatlog.txt");
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
        string LogContent = File.ReadAllText(FilePath);
        Chatlog = JsonSerializer.Deserialize<List<ChatMessage>>(LogContent);
        LogContent = string.Empty; 

        foreach (var line in Chatlog)
        {
            if ((selectedChatType == null || selectedChatType == XivChatType.None || line.ChatType == selectedChatType) &&
            (string.IsNullOrEmpty(searchQuery) || line.Message.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)))
            {
            ImGui.Separator();
            ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1.0f), $"[{line.Timestamp}]");
            ImGui.SameLine();
            ImGui.TextColored(GetColorForType(line.ChatType.ToString()), $"[{line.ChatType}]");
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(0.0f, 0.5f, 1.0f, 1.0f), $"{line.Sender}:");
            ImGui.SameLine();
            ImGui.TextWrapped(line.Message);
            }
        }

        ImGui.EndChild();
        Chatlog.Clear();
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
        Chatlog.Clear();
    }
}