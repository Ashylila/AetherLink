using AetherLink.DalamudServices;
using Dalamud.Plugin.Services;
using System;

namespace AetherLink.Utility
{
    /// <summary>
    /// Class <c>ChatMessageSender</c> is a utility class that sends messages to the chat.
    /// </summary>
    public static class ChatMessageSender
    {
        private static readonly IPluginLog _logger = Svc.Log;
        /// <summary>
        /// Method <c>SendMessage</c> sends a message into the chat.
        /// </summary>
        /// <param name="message"></param>
        public static void SendSayMessage(string message)
        {
            // Use the /say command to send a message that others can see
            string command = $"/say {message}";

            try
            {
                Svc.Framework.RunOnFrameworkThread(() =>
                {
                    Chat.Instance.ExecuteCommand(command);
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send message");
            }
        }

        public static void SendChatMessage(string message)
        {
            // Use the /say command to send a message that others can see
            string command = $"{message}";

            try
            {
                Svc.Framework.RunOnFrameworkThread(() =>
                {
                    Chat.Instance.SendMessage(command);
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send message");
            }
        }
        /// <summary>
        /// Method <c>SendShoutMessage</c> sends a message into the shout chat.
        /// </summary>
        /// <param name="message"></param>
        public static void SendShoutMessage(string message)
        {
            // Use the /shout command to send a message that others can see
            string command = $"/shout {message}";
            try
            {
                Svc.Framework.RunOnFrameworkThread(() =>
                {
                    Chat.Instance.ExecuteCommand(command);
                });
                _logger.Debug($"Sent shout message: {command}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send shout message");
            }
        }
        /// <summary>
        /// Method <c>SendPartyMessage</c> sends a message into the party chat.
        /// </summary>
        /// <param name="message"></param>
        public static void SendPartyMessage(string message)
        {
            try
            {
                Svc.Framework.RunOnFrameworkThread(() =>
                {
                    string command = $"/party {message}";
                    Chat.Instance.SendMessage(command);
                    _logger.Debug($"Sent party message: {command}");
                });
                string command = $"/party {message}";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send party message");
            }
        }
        /// <summary>
        /// Method <c>SendTellMessage</c> sends a private message to a player.
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="message"></param>
        public static void SendTellMessage(string playerName, string message)
        {
            try
            {
                Svc.Framework.RunOnFrameworkThread(() =>
                {
                    string command = $"/tell {playerName} {message}";
                    Chat.Instance.SendMessage(command);
                    _logger.Debug($"Sent tell message: '{command}' to: '{playerName}'");
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send tell message");
            }
        }
        /// <summary>
        /// Method <c>SendFreeCompanyMessage</c> sends a message into the free company chat.
        /// </summary>
        /// <param name="message"></param>
        public static void SendFreeCompanyMessage(string message)
        {
            try
            {
                string command = $"/fc {message}";
                Svc.Framework.RunOnFrameworkThread(() =>
                {
                    Chat.Instance.SendMessage(command);
                    _logger.Debug($"Sent free company message: {command}");
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"An error has occurred while sending the message: {ex.Message}");
            }
        }
    }
}
