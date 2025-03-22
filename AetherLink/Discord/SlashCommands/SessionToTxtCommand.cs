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

namespace AetherLink.Discord.SlashCommands;
public class SessionToTxtCommand : ICommand
{
    public string Name => "sessiontotxt";
    public string Description => "Send the current chat log session to DM.";
    public List<CommandOption> Options { get; } = new List<CommandOption>();

    public async Task Execute(SocketInteraction interaction)
    {
        if (interaction is SocketSlashCommand command)
        {
            try
            {
                string filePath = Path.Combine(Svc.PluginInterface.AssemblyLocation.Directory.FullName, "chatlog.txt");
                string chatLog = File.ReadAllText(filePath);
                List<ChatMessage> chatJson = JsonSerializer.Deserialize<List<ChatMessage>>(chatLog) ?? new List<ChatMessage>();

                // Create a temp file for the formatted chat log
                string tempFilePath = Path.GetTempFileName();
                using (StreamWriter writer = new StreamWriter(tempFilePath))
                {
                    foreach (var message in chatJson)
                    {
                        writer.WriteLine($"[{message.Timestamp}][{message.ChatType}] {message.Sender}: {message.Message}");
                    }
                }

                // Send file as attachment
                await interaction.RespondWithFileAsync(tempFilePath, "chatlog.txt", "Here is the chat log session.");

                // Optional: Delete the temp file after sending
                File.Delete(tempFilePath);
            }
            catch (Exception ex)
            {
                Svc.Log.Error(ex, "Failed to send session to DM");
            }
        }
    }
}
