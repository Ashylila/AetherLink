using System;
using System.Numerics;
using AetherLink.DalamudServices;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;

namespace AetherLink.Windows;

public class ConfigWindow : Window, IDisposable
{
    private IPluginLog Log => Svc.Log;
    private readonly Plugin plugin;
    private readonly Configuration cfg;

    private bool initialized;
    private string editDiscordUserId = string.Empty;
    private string editDiscordBotToken = string.Empty;
    private bool showToken;

    private bool HasChanges =>
        (cfg.DiscordToken ?? string.Empty) != (editDiscordBotToken ?? string.Empty) ||
        cfg.DiscordUserId.ToString() != (editDiscordUserId ?? string.Empty);

    public ConfigWindow(Plugin plugin) : base("AetherLink Config###AetherLinkConfig")
    {
        Flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        Size = new Vector2(620, 180) * ImGuiHelpers.GlobalScale;
        SizeCondition = ImGuiCond.Appearing;
        this.plugin = plugin;
        cfg = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (!initialized)
        {
            editDiscordUserId = cfg.DiscordUserId == 0 ? string.Empty : cfg.DiscordUserId.ToString();
            editDiscordBotToken = cfg.DiscordToken ?? string.Empty;
            initialized = true;
        }

        var scale = ImGuiHelpers.GlobalScale;
        var labelW = 150f * scale;
        var inputW = 360f * scale;

        if (ImGui.BeginTable("cfgtable", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.PadOuterX))
        {
            ImGui.TableSetupColumn("label", ImGuiTableColumnFlags.WidthFixed, labelW);
            ImGui.TableSetupColumn("value", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.TextUnformatted("Discord Bot Token");
            ImGui.TableSetColumnIndex(1);
            ImGui.PushItemWidth(inputW);
            var tokenFlags = showToken ? ImGuiInputTextFlags.None : ImGuiInputTextFlags.Password;
            ImGui.InputText("##token", ref editDiscordBotToken, 256, tokenFlags);
            ImGui.PopItemWidth();
            ImGui.SameLine();
            var right = ImGui.GetContentRegionAvail().X;
            var chkW = 70f * scale;
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + right - chkW);
            ImGui.Checkbox("Show", ref showToken);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.TextUnformatted("Your Discord User ID");
            ImGui.TableSetColumnIndex(1);
            ImGui.PushItemWidth(inputW);
            ImGui.InputText("##userid", ref editDiscordUserId, 32);
            ImGui.PopItemWidth();

            ImGui.EndTable();
        }

        var userIdOk = string.IsNullOrWhiteSpace(editDiscordUserId) || ulong.TryParse(editDiscordUserId, out _);
        if (!userIdOk)
        {
            ImGui.Spacing();
            ImGui.TextColored(new Vector4(1f, 0.5f, 0.5f, 1f), "User ID must be numeric.");
        }

        ImGui.Spacing();

        var region = ImGui.GetContentRegionAvail().X;
        var btnW = 120f * scale;
        var gap = 8f * scale;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + region - (btnW * 2f) - gap);

        var canSave = HasChanges && userIdOk;
        if (!canSave) ImGui.BeginDisabled();
        if (ImGui.Button("Save", new Vector2(btnW, 0)))
        {
            cfg.DiscordUserId = string.IsNullOrWhiteSpace(editDiscordUserId) ? 0 : ulong.Parse(editDiscordUserId);
            cfg.DiscordToken = editDiscordBotToken ?? string.Empty;
            cfg.Save();
            Log.Debug($"Saved config. DiscordUserId={cfg.DiscordUserId}");
        }

        if (!canSave) ImGui.EndDisabled();

        ImGui.SameLine();

        var cfgValid = !string.IsNullOrWhiteSpace(cfg.DiscordToken) &&
                       (cfg.DiscordUserId == 0 || cfg.DiscordUserId > 0);
        if (!cfgValid) ImGui.BeginDisabled();
        if (ImGui.Button("Restart", new Vector2(btnW, 0)))
            plugin.Restart();
        if (!cfgValid) ImGui.EndDisabled();
    }
}
