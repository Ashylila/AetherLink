using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

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
    public string ActiveChannel { get; set; } = string.Empty;

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
    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
