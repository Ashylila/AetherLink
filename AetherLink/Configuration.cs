using Dalamud.Configuration;
using Dalamud.Game.Text;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Text;

namespace AetherLink;

[Serializable]
public class Configuration : IPluginConfiguration
{
    private string discordToken = string.Empty;
    private ulong discordUserId = 0;

    public event Action<string>? OnDiscordTokenChanged;
    public event Action<ulong>? OnDiscordUserIdChanged;
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public string DiscordToken
    {
        get => discordToken;
        set
        {
            if (discordToken != value)
            {
                discordToken = value;
                OnDiscordTokenChanged?.Invoke(value);
            }
        }
    }

    public ulong DiscordUserId
    {
        get => discordUserId;
        set
        {
            if (discordUserId != value)
            {
                discordUserId = value;
                OnDiscordUserIdChanged?.Invoke(value);
            }
        }
    }
    public bool IsRunning { get; set; } = false;
    public bool IsFirstSetup { get; set; } = true;
    public bool IsChatLogEnabled { get; set; } = true;
    public List<XivChatType> ChatTypes { get; set; } = new();
    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
