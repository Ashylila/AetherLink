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
            Logger.Info("Bot logged in successfully!");
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
                            await SendSessionToDM(interaction);
                            return;
                        case "sendmessage":
                            var chatmessage = command.Data.Options.FirstOrDefault(x => x.Name == "message")?.Value as string;
                            chat.SendChatMessage(chatmessage);
                            await interaction.RespondAsync($"Message has been sent to the chat: {chatmessage}", ephemeral: true);
                            await Task.Delay(5000);
                            await interaction.DeleteOriginalResponseAsync();
                            return;
                        case "currentflags":

                            var flags = string.Join("\n", plugin.Configuration.ChatTypes.Select(flag => $"• {flag}"));
                                    var embed = new EmbedBuilder()
                                        .WithTitle("Here are the current active flags:")
                                        .WithDescription(flags)
                                        .WithColor(Color.Blue)
                                        .Build();
                            await interaction.RespondAsync(embed: embed);
                            return;
                        case "help":
                            var helpEmbed = new EmbedBuilder()
                                .WithTitle("Available commands")
                                .AddField("fc", "Send a message to the Free Company")
                                .AddField("tell", "Send a message to a specific person")
                                .AddField("say", "Send a message to the say chat")
                                .AddField("reply", "Reply to the most recent tell message")
                                .AddField("addchatflag", "Add a chat flag")
                                .AddField("removechatflag", "Remove a chat flag")
                                .AddField("sessiontotxt", "Convert the current session to a text file")
                                .AddField("sendmessage", "Send a message into the chat")
                                .AddField("currentflags", "List the current chat flags")
                                .AddField("help", "List all available commands")
                                .AddField("enable", "Enable the bot")
                                .AddField("disable", "Disable the bot")
                                .WithColor(Color.Blue)
                                .Build();
                            await interaction.RespondAsync(embed: helpEmbed);
                            return;
                        case "enable":
                            if(configuration.IsChatLogEnabled)
                            {
                                await interaction.RespondAsync("The logging of the chat is already enabled", ephemeral: true);
                                await Task.Delay(5000);
                                await interaction.DeleteOriginalResponseAsync();
                                return;
                            }
                            configuration.IsChatLogEnabled = true;
                            await interaction.RespondAsync("The logging of the chat has been enabled", ephemeral: true);
                            await Task.Delay(5000);
                            await interaction.DeleteOriginalResponseAsync();
                            return;
                        case "disable":
                            if(!configuration.IsChatLogEnabled)
                            {
                                await interaction.RespondAsync("The logging of the chat is already disabled", ephemeral: true);
                                await Task.Delay(5000);
                                await interaction.DeleteOriginalResponseAsync();
                                return;
                            }
                            configuration.IsChatLogEnabled = false;
                            await interaction.RespondAsync("The logging of the chat has been disabled", ephemeral: true);
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
            if (!plugin.Configuration.ChatTypes.Contains(type) || !plugin.Configuration.IsChatLogEnabled) return;

            string senderworld = GetHomeWorld(sender.TextValue);
            Logger.Verbose(senderworld);
            var pureSender = sender.TextValue.Replace(senderworld, "");
            Logger.Verbose(pureSender);

            try
            {
                string filePath = Path.Combine(Svc.PluginInterface.AssemblyLocation.Directory.FullName, "chatlog.txt");

                string chatLog;
                if(File.Exists(filePath))
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
        private async Task SendSessionToDM(SocketInteraction interaction)
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
        Logger.Error(ex, "Failed to send session to DM");
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
            var currentFlagsCommand = new SlashCommandBuilder()
                    .WithName("currentflags")
                    .WithDescription("List the current chat flags");
            var helpCommand = new SlashCommandBuilder()
                    .WithName("help")
                    .WithDescription("List all available commands");
            var enableCommand = new SlashCommandBuilder()
                    .WithName("enable")
                    .WithDescription("Enable the bot");
            var disableCommand = new SlashCommandBuilder()
                    .WithName("disable")
                    .WithDescription("Disable the bot");
            try
            {
                var commands = new SlashCommandBuilder[] { fcCommand, tellCommand, sayCommand, replyCommand, addChatFlagCommand, removeChatFlagCommand, sessionToTxtCommand, messageCommand, currentFlagsCommand, helpCommand, enableCommand, disableCommand};
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
            var currentCommands = await discordClient.GetGlobalApplicationCommandsAsync();
            try{
            foreach (var command in slashCommandBuilders)
            {
                if(currentCommands.Any(x => x.Name == command.Name))
                {
                    Logger.Verbose($"Command {command.Name} is already registered, skipping");
                    continue;
                }
                await discordClient.CreateGlobalApplicationCommandAsync(command.Build());
            }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to register slash commands");
            }
        }
        private async Task HandleAutoComplete(SocketAutocompleteInteraction interaction)
        {
            try{
            Logger.Verbose($"Auto complete received, command: {interaction.Data.CommandName}, current: {interaction.Data.Current.Name}");
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
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to handle auto complete");
            }
        }
        private async Task DiscordOnReady()
        {
            Logger.Debug("Discord bot is ready, registering commands...");
            try
            {
                await RegisterSlashCommands();
                chatGui.ChatMessage += OnChatMessage;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to register commands...");
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
