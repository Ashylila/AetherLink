using Dalamud.Plugin.Services;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Game.Text;
using AetherLink.Models;
using AetherLink.Utility;
using AetherLink.DalamudServices;
using Discord.Net;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Serilog;


namespace AetherLink.Discord
{
    public class DiscordHandler : IDisposable
    {
        private static IPluginLog Logger;
        public static List<ChatMessage> chatMessages = new();
        private readonly DiscordSocketClient discordClient;
        private Configuration configuration;
        private IChatGui chatGui;

        public bool isConnected => this.discordClient.ConnectionState == ConnectionState.Connected;
        public DiscordHandler(DiscordSocketClient client, Configuration config, IChatGui chatGui, IPluginLog logger)
        {
            Logger = logger;
            this.chatGui = chatGui;
            this.configuration = config;
            this.discordClient = client;
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
        public void Dispose()
        {
            configuration.OnDiscordTokenChanged -= TokenChanged;
        }
    }
}
