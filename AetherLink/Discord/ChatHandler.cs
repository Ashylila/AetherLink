using Dalamud.Plugin.Services;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using AetherLink.Models;
using AetherLink.Utility;
using AetherLink.DalamudServices;
using System.Text.Json;
using System.IO;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;

namespace AetherLink.Discord;

public class ChatHandler(DiscordSocketClient client, Plugin plugin, IChatGui gui, IPluginLog log, IDataManager data, IClientState clientState, IDalamudPluginInterface Pi) : IDisposable
{
    private readonly IDalamudPluginInterface PluginInterface = Pi;
    private readonly IClientState ClientState = clientState;
    private readonly IDataManager Data = data;
    private readonly IChatGui ChatGui = gui;
    private readonly IPluginLog Logger = log;
    private readonly DiscordSocketClient Client = client;
    private readonly Plugin Plugin = plugin;

    private readonly List<ChatMessage> ChatLog = [];

    private readonly string LogFilePath = Path.Combine(Pi.AssemblyLocation.Directory.FullName, "chatlog.ndjson");

    public void Init()
    {
        LoadExistingChatLog();
        ChatGui.ChatMessage += OnChatMessage;
    }

    public void Dispose()
    {
        ChatGui.ChatMessage -= OnChatMessage;
    }

    private void OnChatMessage(
        XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (!Plugin.Configuration.ChatTypes.Contains(type) || !Plugin.Configuration.IsChatLogEnabled)
            return;

        var playerPayload = sender.Payloads.OfType<PlayerPayload>().FirstOrDefault();
        var pureSender = playerPayload?.PlayerName ?? sender.TextValue;
        var senderWorld = playerPayload?.World.Value.Name.ExtractText() ?? "";

        var chatMessage = new ChatMessage
        {
            Timestamp = DateTime.Now,
            Sender = $"{pureSender}@{senderWorld}",
            Message = message.TextValue,
            ChatType = type
        };

        ChatLog.Add(chatMessage);
        AppendChatMessage(chatMessage);

        DiscordHandler.chatMessages.Add(chatMessage);

        var embed = new EmbedBuilder()
            .WithAuthor($"[{type}] {pureSender}")
            .WithDescription(message.TextValue)
            .WithTimestamp(chatMessage.Timestamp)
            .WithColor(GetColorForType(type))
            .Build();

        _ = Task.Run(async () =>
        {
            try { await SendMessageToDm(embed); }
            catch (Exception ex) { Logger.Error(ex, "Failed to send Discord message."); }
        });
    }

    private void LoadExistingChatLog()
    {
        if (!File.Exists(LogFilePath)) return;

        try
        {
            foreach (var line in File.ReadLines(LogFilePath))
            {
                var msg = JsonSerializer.Deserialize<ChatMessage>(line);
                if (msg != null)
                    ChatLog.Add(msg);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to load chatlog.ndjson");
        }
    }

    private void AppendChatMessage(ChatMessage msg)
    {
        try
        {
            var json = JsonSerializer.Serialize(msg);
            File.AppendAllText(LogFilePath, json + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to append chat message to log");
        }
    }

    private async Task SendMessageToDm(Embed embed)
    {
        if (Plugin.Configuration.DiscordUserId == 0)
        {
            Logger.Error("Discord user ID is not set.");
            ChatGui.Print("Discord user ID is not set. Please set it in the config window.", messageTag: "AetherLink", tagColor: 51447);
            return;
        }

        try
        {
            var user = await Client.GetUserAsync(Plugin.Configuration.DiscordUserId);
            if (user == null)
            {
                Logger.Error("Discord user not found.");
                return;
            }

            var dmChannel = await user.CreateDMChannelAsync();
            if (dmChannel == null)
            {
                Logger.Error("Failed to create DM channel.");
                return;
            }

            await dmChannel.SendMessageAsync(embed: embed);
            Logger.Debug("Message sent to Discord.");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Exception sending DM.");
        }
    }

    private Color GetColorForType(XivChatType type) => type switch
    {
        XivChatType.FreeCompany => Color.Blue,
        XivChatType.Ls1 or XivChatType.Ls2 or XivChatType.Ls3 or XivChatType.Ls4 or
        XivChatType.Ls5 or XivChatType.Ls6 or XivChatType.Ls7 or XivChatType.Ls8 => Color.Green,
        XivChatType.CrossLinkShell1 or XivChatType.CrossLinkShell2 or XivChatType.CrossLinkShell3 => Color.Purple,
        XivChatType.TellIncoming or XivChatType.TellOutgoing => Color.Magenta,
        XivChatType.Say => Color.LighterGrey,
        XivChatType.Shout => Color.Orange,
        XivChatType.Party => Color.Blue,
        _ => Color.LightGrey
    };
}
