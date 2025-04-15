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
using System.Text;
using System.IO;
using Discord.Net;
using Lumina.Excel.Sheets;
using System.Text.Json;
using Dalamud.Plugin;
using Serilog;

namespace AetherLink.Discord;

public class ChatHandler(DiscordSocketClient client, Plugin plugin, IChatGui gui, IPluginLog log, IDataManager data, IClientState clientState, IDalamudPluginInterface PI) : IDisposable
{
    private readonly IDalamudPluginInterface PluginInterface = PI;
    private readonly IClientState ClientState = clientState;
    private readonly IDataManager Data = data;
    private IChatGui ChatGui = gui;
    private IPluginLog Logger = log;
    private DiscordSocketClient client = client;
    private Plugin plugin =  plugin;
    public void Init()
    {
        ChatGui.ChatMessage += OnChatMessage;
    }

    public void Dispose()
    {
        ChatGui.ChatMessage -= OnChatMessage;
    }

    private void OnChatMessage(
        XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if(!plugin.Configuration.ChatTypes.Contains(type) ||  !plugin.Configuration.IsChatLogEnabled) return;
        var senderWorld = GetHomeWorld(sender.ToString());
        var pureSender = sender.TextValue.Replace(senderWorld, "");

        var chatLog = GetChatLog();
        var chatMessage = new ChatMessage
        {
            Timestamp = DateTime.Now,
            Sender = pureSender + "@" + senderWorld,
            Message = message.TextValue,
            ChatType = type
        };
        chatLog.Add(chatMessage);
        SaveChatLog(chatLog);
        DiscordHandler.chatMessages.Add(chatMessage);
        var embed = new EmbedBuilder()
                    .WithAuthor($"[{type}]{pureSender}")
                    .WithDescription(message.TextValue)
                    .WithTimestamp(chatMessage.Timestamp)
                    .WithColor(GetColorForType(type))
                    .Build();
        _ = SendMessageToDm(embed);


    }
    private string GetHomeWorld(string name)
    {
        var WorldList = Data.GetExcelSheet<World>().Select(world => world.Name.ToString()).Where(name => !string.IsNullOrEmpty(name)).ToHashSet();
        string senderworld;
        if (WorldList.Any(world => name.Contains(world, StringComparison.OrdinalIgnoreCase)))
        {
            senderworld = WorldList.FirstOrDefault(world => name.Contains(world, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            senderworld = ClientState.LocalPlayer.HomeWorld.Value.Name.ToString();
        }
        return senderworld ?? "World not found";
    }

    private List<ChatMessage> GetChatLog()
    {
        string filePath = Path.Combine(PluginInterface.AssemblyLocation.Directory.FullName, "chatlog.txt");
        string chatLog;
        if (File.Exists(filePath))
        {
            chatLog = File.ReadAllText(filePath);
        }
        else
        {
            chatLog = "[]";
        }

        return JsonSerializer.Deserialize<List<ChatMessage>>(chatLog) ?? new List<ChatMessage>();
    }

    private async Task SendMessageToDm(Embed embed)
    {
        if (plugin.Configuration.DiscordUserId == 0)
        {
            Logger.Error("Discord user id is not set, cannot send message");
            ChatGui.Print("Discord user id is not set, please set it in the config window", messageTag: "AetherLink", tagColor: 51447);
            return;
        }
        try
        {
            var user = await client.GetUserAsync(plugin.Configuration.DiscordUserId);
            if (user != null)
            {
                var dmChannel = await user.CreateDMChannelAsync();
                if (dmChannel != null)
                {
                    await dmChannel.SendMessageAsync(embed: embed);
                    Logger.Debug("Message sent");
                }
                else
                {
                    Logger.Error("Failed to create DM channel");
                }
            }
            else
            {
                Logger.Error("User not found");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to send DM with embed");
        }
    }
    
    private void SaveChatLog(List<ChatMessage> chat)
    {
        var filePath = Path.Combine(PluginInterface.AssemblyLocation.Directory.FullName, "chatlog.txt");
        File.WriteAllText(filePath, JsonSerializer.Serialize(chat, new JsonSerializerOptions { WriteIndented = true }));
    }
    private Color GetColorForType(XivChatType type)
    {
        return type switch
        {
            XivChatType.FreeCompany => Color.Blue,
            XivChatType.Ls1 => Color.Green,
            XivChatType.Ls2 => Color.Green,
            XivChatType.Ls3 => Color.Green,
            XivChatType.Ls4 => Color.Green,
            XivChatType.Ls5 => Color.Green,
            XivChatType.Ls6 => Color.Green,
            XivChatType.Ls7 => Color.Green,
            XivChatType.Ls8 => Color.Green,
            XivChatType.CrossLinkShell1 => Color.Purple,
            XivChatType.CrossLinkShell2 => Color.Purple,
            XivChatType.CrossLinkShell3 => Color.Purple,
            XivChatType.TellIncoming => Color.Magenta,
            XivChatType.TellOutgoing => Color.Magenta,
            XivChatType.Say => Color.LighterGrey,
            XivChatType.Shout => Color.Orange,
            XivChatType.Party => Color.Blue,
            _ => Color.LightGrey,
        };
    }
    
}
