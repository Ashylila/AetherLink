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
    public StringBuilder ChatLog { get; set; } = new();
    public List<XivChatType> ChatTypes { get; set; } = new()
    {
            XivChatType.FreeCompany,
            XivChatType.Ls1,
            XivChatType.Ls2,
            XivChatType.Ls3,
            XivChatType.Ls4,
            XivChatType.Ls5,
            XivChatType.Ls6,
            XivChatType.Ls7,
            XivChatType.Ls8,
            XivChatType.CrossLinkShell1,
            XivChatType.CrossLinkShell2,
            XivChatType.CrossLinkShell3,
            XivChatType.TellIncoming,
            XivChatType.TellOutgoing,
            XivChatType.Say
    };
    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
