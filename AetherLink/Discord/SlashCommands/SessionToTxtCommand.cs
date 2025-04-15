using System.Threading.Tasks;
using AetherLink.Models;
using Discord.WebSocket;
using System.Linq;
using AetherLink.Utility;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using AetherLink.DalamudServices;
using System;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Discord.Interactions;

namespace AetherLink.Discord.SlashCommands;
public class SessionToTxtCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IPluginLog _log;
    private readonly IDalamudPluginInterface _interface;

    public SessionToTxtCommand(IPluginLog log, IDalamudPluginInterface pluginInterface)
    {
        _log = log;
        _interface = pluginInterface;
    }
    [SlashCommand("sessiontotxt", "Send the current chat log session to DM")]
    public async Task Execute()
    {
            try
            {
                string filePath = Path.Combine(_interface.AssemblyLocation.Directory.FullName, "chatlog.txt");
                string chatLog = File.ReadAllText(filePath);
                List<ChatMessage> chatJson = JsonSerializer.Deserialize<List<ChatMessage>>(chatLog) ?? new List<ChatMessage>();

                
                string tempFilePath = Path.GetTempFileName();
                await using (StreamWriter writer = new StreamWriter(tempFilePath))
                {
                    foreach (var message in chatJson)
                    {
                        writer.WriteLine($"[{message.Timestamp}][{message.ChatType}] {message.Sender}: {message.Message}");
                    }
                }

                
                await RespondWithFileAsync(tempFilePath, "chatlog.txt", "Here is the chat log session.");
                File.Delete(tempFilePath);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to send session to DM");
            }
    }
}
