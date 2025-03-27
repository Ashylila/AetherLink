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
using Serilog;


namespace AetherLink.Discord
{
    public class DiscordHandler : IDisposable
    {
        static IPluginLog Logger => Svc.Log;
        public static List<ChatMessage> chatMessages = new();
        internal readonly DiscordSocketClient discordClient;
        private Configuration configuration => Svc.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        private IChatGui chatGui => Svc.Chat;
        private readonly Plugin plugin;

        public bool isConnected => this.discordClient.ConnectionState == ConnectionState.Connected;
        public ulong UserId => this.discordClient.CurrentUser.Id;
        public DiscordHandler(Plugin plugin)
        {
            this.plugin = plugin;
            
            this.discordClient = new(new DiscordSocketConfig()
            {
                MessageCacheSize = 20,
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMessages | GatewayIntents.GuildWebhooks | GatewayIntents.MessageContent,
            });
            configuration.OnDiscordTokenChanged += TokenChanged;
        }
        public async Task TokenChanged(string changedValue)
        {
            chatGui.Print("Token changed, restarting bot", messageTag: "AetherLink", tagColor: 51447);
            await discordClient.StopAsync();
            await _init();
        }
        public async Task _init()
        {
            Logger.Debug("Starting discord bot...");
            if (string.IsNullOrEmpty(configuration.DiscordToken))
            {
                Logger.Error("Discord token is not set, cannot start bot");
                chatGui.Print("Discord token is not set, cannot start bot", messageTag: "AetherLink", tagColor: 51447);
                return;
            }
            try
            {
                await this.discordClient.LoginAsync(TokenType.Bot, configuration.DiscordToken);
                await this.discordClient.StartAsync();
                this.discordClient.AutocompleteExecuted += HandleAutoComplete;
                Log.Debug("Subscribed to Chatmessage event...");
                chatGui.ChatMessage += OnChatMessage;
            }
            catch (HttpException httpEx)
            {
                Logger.Error(httpEx, "HTTP error occurred while starting bot.");
                chatGui.Print("HTTP error occurred while starting bot.", messageTag: "AetherLink", tagColor: 51447);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occurred while starting bot.");
                chatGui.Print("An error occurred while starting bot.", messageTag: "AetherLink", tagColor: 51447);
            }
            Logger.Info("Bot logged in successfully!");
        }
        private void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if(!isConnected)
            {
            Logger.Error("Bot is not connected");
            return;
            }
            if (!configuration.ChatTypes.Contains(type) || !configuration.IsChatLogEnabled) return;

            string senderworld = GetHomeWorld(sender.TextValue);
            var pureSender = sender.TextValue.Replace(senderworld, "");

            try
            {
                string filePath = Path.Combine(Svc.PluginInterface.AssemblyLocation.Directory.FullName, "chatlog.txt");

                string chatLog;
                if (File.Exists(filePath))
                {
                    chatLog = File.ReadAllText(filePath);
                }
                else
                {
                    chatLog = "[]";
                }

                List<ChatMessage> chatJson = JsonSerializer.Deserialize<List<ChatMessage>>(chatLog) ?? new List<ChatMessage>();

                var Message = new ChatMessage()
                {
                    Sender = pureSender + "@" + senderworld,
                    Message = message.TextValue,
                    Timestamp = DateTime.Now,
                    ChatType = type
                };
                chatJson.Add(Message);
                File.WriteAllText(filePath, JsonSerializer.Serialize(chatJson, new JsonSerializerOptions { WriteIndented = true }));
                //plugin.Configuration.ChatLog.AppendLine($"[{Message.Timestamp}][{type}] {Message.Sender}: {Message.Message}");

                var embed = new EmbedBuilder()
                    .WithAuthor($"[{type}]{pureSender}")
                    .WithDescription(message.TextValue)
                    .WithTimestamp(Message.Timestamp)
                    .WithColor(GetColorForType(type))
                    .Build();

                chatMessages.Add(Message);


                Task.Run(async () => SendEmbedToDM(embed));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to send message");
            }
        }

        private async Task SendEmbedToDM(Embed embed)
        {
            if (configuration.DiscordUserId == 0)
            {
                Logger.Error("Discord user id is not set, cannot send message");
                chatGui.Print("Discord user id is not set, please set it in the config window", messageTag: "AetherLink", tagColor: 51447);
                return;
            }
            try
            {
                var user = await this.discordClient.GetUserAsync(configuration.DiscordUserId);
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
        private async Task HandleAutoComplete(SocketAutocompleteInteraction interaction)
        {
            try
            {
                Logger.Verbose($"Auto complete received, command: {interaction.Data.CommandName}, current: {interaction.Data.Current.Name}");
                if (interaction.Data.CommandName == "tell" && interaction.Data.Current.Name == "target")
                {
                    string userInput = interaction.Data.Current.Value.ToString();

                    var suggestions = chatMessages.Where(message => message.Sender.StartsWith(userInput, StringComparison.OrdinalIgnoreCase)).Select(message => new AutocompleteResult(message.Sender, message.Sender)).Take(5).ToList();
                    await interaction.RespondAsync(suggestions);
                }
                else if ((interaction.Data.CommandName == "addchatflag" || interaction.Data.CommandName == "removechatflag") && interaction.Data.Current.Name == "flag")
                {
                    string userInput = interaction.Data.Current.Value.ToString();
                    var choices = EnumHelper.GetEnumChoices<XivChatType>().Where(flag => flag.StartsWith(userInput, StringComparison.OrdinalIgnoreCase)).Select(flag => new AutocompleteResult(flag, flag)).Take(5).ToList();
                    await interaction.RespondAsync(choices);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to handle auto complete");
            }
        }
        private string GetHomeWorld(string name)
        {
            var WorldList = Svc.Data.GetExcelSheet<World>().Select(world => world.Name.ToString()).Where(name => !string.IsNullOrEmpty(name)).ToHashSet();
            string senderworld;
            if (WorldList.Any(world => name.Contains(world, StringComparison.OrdinalIgnoreCase)))
            {
                senderworld = WorldList.FirstOrDefault(world => name.Contains(world, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                senderworld = Svc.ClientState.LocalPlayer.HomeWorld.Value.Name.ToString();
            }
            return senderworld ?? "World not found";
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
        public void Dispose()
        {
            chatGui.ChatMessage -= OnChatMessage;
            if (discordClient != null)
            {
                discordClient.Dispose();
            }

        }
    }
}
