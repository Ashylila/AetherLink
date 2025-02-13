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
using AetherLink.Constants;
using System.Text;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Discord.Net;


namespace AetherLink.Discord
{
    public class DiscordHandler : IDisposable
    {
        private ChatMessageSender chat;
        static IPluginLog Logger => Svc.Log;
        public List<ChatMessage> chatMessages = new();
        private readonly DiscordSocketClient discordClient;
        private readonly Configuration configuration;
        private readonly IChatGui chatGui;
        private readonly Plugin plugin;

        public bool isConnected => this.discordClient.ConnectionState == ConnectionState.Connected;
        public ulong UserId => this.discordClient.CurrentUser.Id;
        public DiscordHandler(Plugin plugin)
        {
            this.plugin = plugin;
            configuration = plugin.Configuration;
            chat = new ChatMessageSender();
            this.discordClient = new(new DiscordSocketConfig()
            {
                MessageCacheSize = 20,
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMessages | GatewayIntents.GuildWebhooks | GatewayIntents.MessageContent,
            });
            configuration.OnDiscordTokenChanged += TokenChanged;
            chatGui = Svc.Chat;
            configuration.IsRunning = isConnected;
        }
        public async void TokenChanged(string changedValue)
        {
            chatGui.Print("Token changed, restarting bot", messageTag: "AetherLink", tagColor: 51447);
            await discordClient.StopAsync();
            await _init();
        }
        public async Task _init()
        {
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
                this.discordClient.Ready += DiscordOnReady;
                this.discordClient.InteractionCreated += HandleInteractionAsync;
                this.discordClient.AutocompleteExecuted += HandleAutoComplete;
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
            Logger.Info("DiscordHandler initialized");
        }
        private async Task HandleInteractionAsync(SocketInteraction interaction)
        {
            Logger.Debug("Interaction received");
            if (interaction is SocketSlashCommand command)
            {
                try
                {
                    switch (command.CommandName)
                    {
                        case "tell":
                            var target = command.Data.Options.FirstOrDefault(x => x.Name == "target")?.Value as string;
                            var message = command.Data.Options.FirstOrDefault(x => x.Name == "message")?.Value as string;
                            chat.SendTellMessage(target, message);
                            await interaction.RespondAsync($"responded to:{target} with {message}", ephemeral: true);
                            await Task.Delay(5000);
                            await interaction.DeleteOriginalResponseAsync();
                            return;
                        case "fc":
                            var fcmessage = command.Data.Options.FirstOrDefault(x => x.Name == "message")?.Value as string;
                            chat.SendFreeCompanyMessage(fcmessage);
                            await interaction.RespondAsync($"Message has been sent to the Free Company: {fcmessage}", ephemeral: true);
                            await Task.Delay(5000);
                            await interaction.DeleteOriginalResponseAsync();
                            return;
                        case "say":
                            var saymessage = command.Data.Options.FirstOrDefault(x => x.Name == "message")?.Value as string;
                            chat.SendSayMessage(saymessage);
                            await interaction.RespondAsync($"Message has been sent to the say chat: {saymessage}", ephemeral: true);
                            await Task.Delay(5000);
                            await interaction.DeleteOriginalResponseAsync();
                            return;
                        case "reply":
                            var replymessage = command.Data.Options.FirstOrDefault(x => x.Name == "message")?.Value as string;
                            chat.SendChatMessage("/r " + replymessage);
                            await interaction.RespondAsync($"responded to:{command.Data.Options.FirstOrDefault(x => x.Name == "target")?.Value} with {replymessage}", ephemeral: true);
                            await Task.Delay(5000);
                            await interaction.DeleteOriginalResponseAsync();
                            return;
                        case "removechatflag":
                            var flagToRemove = command.Data.Options.FirstOrDefault(x => x.Name == "flag")?.Value as string;
                            if (!EnumHelper.IsValidEnumMember<XivChatType>(flagToRemove) || (EnumHelper.TryConvertToEnum<XivChatType>(flagToRemove, out var result) && !plugin.Configuration.ChatTypes.Contains(result)))
                            {
                                await interaction.RespondAsync("Invalid flag or it is already inactive.", ephemeral: true);
                                await Task.Delay(5000);
                                await interaction.DeleteOriginalResponseAsync();
                                return;
                            }
                            Logger.Debug($"Flag: {flagToRemove}");
                            plugin.Configuration.ChatTypes.Remove(result);
                            await interaction.RespondAsync($"Flag {flagToRemove} has been removed", ephemeral: true);
                            await Task.Delay(5000);
                            await interaction.DeleteOriginalResponseAsync();
                            return;
                        case "addchatflag":
                            var flag = command.Data.Options.FirstOrDefault(x => x.Name == "flag")?.Value as string;
                            if (!EnumHelper.IsValidEnumMember<XivChatType>(flag) || (EnumHelper.TryConvertToEnum<XivChatType>(flag, out var addresult) && plugin.Configuration.ChatTypes.Contains(addresult)))
                            {
                                await interaction.RespondAsync("Invalid flag or it is already active.", ephemeral: true);
                                await Task.Delay(5000);
                                await interaction.DeleteOriginalResponseAsync();
                                return;
                            }
                            Logger.Debug($"Flag: {flag}");
                                plugin.Configuration.ChatTypes.Add(addresult);
                            await interaction.RespondAsync($"Flag {flag} has been added", ephemeral: true);
                            await Task.Delay(5000);
                            await interaction.DeleteOriginalResponseAsync();
                            return;
                        case "sessiontotxt":
                            if (plugin.Configuration.ChatLog.Length == 0)
                            {
                                await interaction.RespondAsync("No messages to convert", ephemeral: true);
                                await Task.Delay(5000);
                                await interaction.DeleteOriginalResponseAsync();
                                return;
                            }
                            await SendSessionToDM(interaction);
                            return;
                        case "sendmessage":
                            var chatmessage = command.Data.Options.FirstOrDefault(x => x.Name == "message")?.Value as string;
                            chat.SendChatMessage(chatmessage);
                            await interaction.RespondAsync($"Message has been sent to the chat: {chatmessage}", ephemeral: true);
                            await Task.Delay(5000);
                            await interaction.DeleteOriginalResponseAsync();
                            return;
                    };
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to handle interaction");
                }
            };
            await interaction.RespondAsync("Command not found", ephemeral: true);
        }
        private void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (!plugin.Configuration.ChatTypes.Contains(type))
            {
                return;
            }
            Logger.Verbose($"Chat message received: {sender.TextValue}: {message.TextValue}, type: {type}");

            string senderworld = string.Empty;
            var pureSender = sender.TextValue;
            senderworld = GetHomeWorld(pureSender);
            pureSender = pureSender.Replace(senderworld, "");
            Logger.Debug(senderworld);
            try
            {
                var Message = new ChatMessage()
                {
                    Sender = pureSender + "@" + senderworld,
                    Message = message.TextValue,
                    Timestamp = DateTime.Now,
                    ChatType = type
                };
                plugin.Configuration.ChatLog.AppendLine($"[{Message.Timestamp}][{type}] {Message.Sender}: {Message.Message}");
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
        private async Task SendSessionToDM(SocketInteraction interaction)
        {
            if (string.IsNullOrWhiteSpace(plugin.Configuration.ChatLog.ToString()))
            {
                Logger.Error("No messages to send.");
                return;
            }

            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(plugin.Configuration.ChatLog.ToString()));
            plugin.Configuration.ChatLog.Clear();

            try
            {
                var attachment = new FileAttachment(stream, "session.txt");
                await interaction.RespondWithFileAsync(attachment, "Here's the log for the current session!");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to send session to DM");
            }
            finally
            {
                stream.Dispose();
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

        private async Task RegisterSlashCommands()
        {
            var fcCommand = new SlashCommandBuilder()
                    .WithName("fc")
                    .WithDescription("Send a message to the Free Company")
                    .AddOption("message", ApplicationCommandOptionType.String, "The message to send", isRequired:true);
            var tellCommand = new SlashCommandBuilder()
                .WithName("tell")
                .WithDescription("Send a message to a specific person")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("target")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithDescription("The target to send the message to")
                    .WithAutocomplete(true)
                    .WithRequired(true))
                .AddOption("message", ApplicationCommandOptionType.String, "The message to send");
            var sayCommand = new SlashCommandBuilder()
                    .WithName("say")
                    .WithDescription("Send a message to the say chat")
                    .AddOption("message", ApplicationCommandOptionType.String, "The message to send", isRequired:true);
            var replyCommand = new SlashCommandBuilder()
                    .WithName("reply")
                    .WithDescription("Reply to the most recent tell message")
                    .AddOption("message", ApplicationCommandOptionType.String, "The message to send", isRequired:true);
            var addChatFlagCommand = new SlashCommandBuilder()
                    .WithName("addchatflag")
                    .WithDescription("Add a chat flag")
                    .AddOption("flag",ApplicationCommandOptionType.String, "the flag to add", isAutocomplete:true, isRequired:true);
            var removeChatFlagCommand = new SlashCommandBuilder()
                    .WithName("removechatflag")
                    .WithDescription("Remove a chat flag")
                    .AddOption("flag", ApplicationCommandOptionType.String, "The flag to remove", isAutocomplete:true, isRequired:true);
            var sessionToTxtCommand = new SlashCommandBuilder()
                    .WithName("sessiontotxt")
                    .WithDescription("Convert the current session to a text file");
            var messageCommand = new SlashCommandBuilder()
                    .WithName("sendmessage")
                    .WithDescription("Send a message into the chat")
                    .AddOption("message", ApplicationCommandOptionType.String, "The message to send", isRequired: true);

            try
            {

                var commands = new SlashCommandBuilder[] { fcCommand, tellCommand, sayCommand, replyCommand, addChatFlagCommand, removeChatFlagCommand, sessionToTxtCommand, messageCommand };
                await RegisterBulkCommands(commands);

                Logger.Debug("commands registered");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to register slash commands");
            }
        }
        private async Task RegisterBulkCommands(SlashCommandBuilder[] slashCommandBuilders)
        {
            foreach (var command in slashCommandBuilders)
            {
                await discordClient.CreateGlobalApplicationCommandAsync(command.Build());
            }
        }
        private async Task HandleAutoComplete(SocketAutocompleteInteraction interaction)
        {
            if (interaction.Data.CommandName == "tell" && interaction.Data.Current.Name == "target")
            {
                string userInput = interaction.Data.Current.Value.ToString();

                var suggestions = chatMessages.Where(message => message.Sender.StartsWith(userInput, StringComparison.OrdinalIgnoreCase)).Select(message => new AutocompleteResult(message.Sender, message.Sender)).Take(5).ToList();
                await interaction.RespondAsync(suggestions);
            }
            else if ((interaction.Data.CommandName == "addchatflag" || interaction.Data.CommandName =="removechatflag") && interaction.Data.Current.Name == "flag")
            {
                string userInput = interaction.Data.Current.Value.ToString();
                var choices = EnumHelper.GetEnumChoices<XivChatType>().Where(flag => flag.StartsWith(userInput, StringComparison.OrdinalIgnoreCase)).Select(flag => new AutocompleteResult(flag, flag)).Take(5).ToList();
                await interaction.RespondAsync(choices);
            }
        }
        private async Task DiscordOnReady()
        {
            try
            {
                await RegisterSlashCommands();
                chatGui.ChatMessage += OnChatMessage;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to add modules");
            }
        }
        private static string GetHomeWorld(string name)
        {
            string senderworld;
            if (WorldList.worldList.Any(world => name.Contains(world, StringComparison.OrdinalIgnoreCase)))
            {
                senderworld = WorldList.worldList.FirstOrDefault(world => name.Contains(world, StringComparison.OrdinalIgnoreCase));
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
