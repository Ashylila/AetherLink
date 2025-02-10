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
        StringBuilder messages = new();
        private readonly List<XivChatType> chatTypes = [
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
            XivChatType.Say];

        public bool isConnected => this.discordClient.ConnectionState == ConnectionState.Connected;
        public ulong UserId => this.discordClient.CurrentUser.Id;
        public DiscordHandler(Plugin plugin)
        {
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
            await discordClient.StopAsync();
            await _init();
        }
        public async Task _init()
        {
            if (string.IsNullOrEmpty(configuration.DiscordToken))
            {
                Logger.Error("Discord token is not set, cannot start bot");
                chatGui.Print("Discord token is not set, cannot start bot");
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
            catch (Exception ex)
            {
                Logger.Error(ex, "Token invalid, cannot start bot.");
                chatGui.Print("Token invalid, cannot start bot.");
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
                            if (!EnumHelper.IsValidEnumMember<XivChatType>(flagToRemove) || (EnumHelper.TryConvertToEnum<XivChatType>(flagToRemove, out var result) && !chatTypes.Contains(result)))
                            {
                                await interaction.RespondAsync("Invalid flag or it is already inactive.", ephemeral: true);
                                await Task.Delay(5000);
                                await interaction.DeleteOriginalResponseAsync();
                                return;
                            }
                            Logger.Debug($"Flag: {flagToRemove}");
                            chatTypes.Remove(result);
                            await interaction.RespondAsync($"Flag {flagToRemove} has been removed", ephemeral: true);
                            await Task.Delay(5000);
                            await interaction.DeleteOriginalResponseAsync();
                            return;
                        case "addchatflag":
                            var flag = command.Data.Options.FirstOrDefault(x => x.Name == "flag")?.Value as string;
                            if (!EnumHelper.IsValidEnumMember<XivChatType>(flag) || (EnumHelper.TryConvertToEnum<XivChatType>(flag, out var addresult) && chatTypes.Contains(addresult)))
                            {
                                await interaction.RespondAsync("Invalid flag or it is already active.", ephemeral: true);
                                await Task.Delay(5000);
                                await interaction.DeleteOriginalResponseAsync();
                                return;
                            }
                            Logger.Debug($"Flag: {flag}");
                                chatTypes.Add(addresult);
                            await interaction.RespondAsync($"Flag {flag} has been added", ephemeral: true);
                            await Task.Delay(5000);
                            await interaction.DeleteOriginalResponseAsync();
                            return;
                        case "sessiontotxt":
                            if (messages.Length == 0)
                            {
                                await interaction.RespondAsync("No messages to convert", ephemeral: true);
                                await Task.Delay(5000);
                                await interaction.DeleteOriginalResponseAsync();
                                return;
                            }
                            await SendSessionToDM();
                            await interaction.RespondAsync("done", ephemeral:true);
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
            if (!chatTypes.Contains(type))
            {
                return;
            }
            Logger.Verbose($"Chat message received: {sender.TextValue}: {message.TextValue}, type: {type}");

            string senderworld = string.Empty;
            var pureSender = sender.TextValue;
            if (WorldList.worldList.Any(world => pureSender.Contains(world)))
            {
                senderworld = WorldList.worldList.FirstOrDefault(world => pureSender.Contains(world, StringComparison.OrdinalIgnoreCase));
                pureSender = pureSender.Replace(senderworld, "");
            }
            else
            {
                senderworld = Svc.ClientState.LocalPlayer.HomeWorld.Value.Name.ToString();
            }
            Logger.Debug(senderworld);
            try
            {


                var Message = new ChatMessage()
                {
                    Sender = sender.TextValue + "@" + senderworld,
                    Message = message.TextValue,
                    Timestamp = DateTime.Now,
                    ChatType = type
                };
                messages.AppendLine($"[{Message.Timestamp}][{type}] {Message.Sender}: {Message.Message}");
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
        private async Task SendSessionToDM()
        {
            if (string.IsNullOrWhiteSpace(messages.ToString()))
            {
                Logger.Error("No messages to send.");
                return;
            }
            var user = await discordClient.GetUserAsync(configuration.DiscordUserId);
            var dmChannel = await user.CreateDMChannelAsync();
            if (dmChannel == null)
            {
                Logger.Error("DM channel is null. The user may have never DMed the bot or has DMs disabled.");
                return;
            }

            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(messages.ToString()));
            messages.Clear();

            try
            {
                var attachment = new FileAttachment(stream, "session.txt");
                await dmChannel.SendFileAsync(attachment, "Here's the log for the current session!");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to send session to DM");
            }
        }

        private async Task SendEmbedToDM(Embed embed)
        {
            if(configuration.DiscordUserId == 0)
            {
                Logger.Error("Discord user id is not set, cannot send message");
                chatGui.Print("Discord user id is not set, please set it in the config window");
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
                    .AddOption("flag", ApplicationCommandOptionType.String, "The flag to remove", isAutocomplete:true, isRequired:true)
                    .Build();
            var sessionToTxtCommand = new SlashCommandBuilder()
                    .WithName("sessiontotxt")
                    .WithDescription("Convert the current session to a text file");

            try
            {

               // await discordClient.CreateGlobalApplicationCommandAsync(tellCommand.Build());
                //await discordClient.CreateGlobalApplicationCommandAsync(sayCommand.Build());
                //await discordClient.CreateGlobalApplicationCommandAsync(fcCommand.Build());
                //await discordClient.CreateGlobalApplicationCommandAsync(replyCommand.Build());
                //await discordClient.CreateGlobalApplicationCommandAsync(addChatFlagCommand.Build());
                //await discordClient.CreateGlobalApplicationCommandAsync(removeChatFlagCommand);
                //await discordClient.CreateGlobalApplicationCommandAsync(sessionToTxtCommand.Build());
                Logger.Debug("commands registered");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to register slash commands");
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
                _ => Color.LightGrey,
            };
        }
        public void Dispose()
        {
            discordClient.Dispose();
        }
    }
}
