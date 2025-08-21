using AetherLink.DalamudServices;
using Dalamud.Plugin.Services;
using System;

namespace AetherLink.Utility
{
    /// <summary>
    /// Utility for sending chat messages into FFXIV.
    /// Returns true if sending succeeded, false otherwise.
    /// </summary>
    public static class ChatMessageSender
    {
        private static readonly IPluginLog _logger = Svc.Log;

        public static bool SendSayMessage(string message)
        {
            var command = $"/say {message}";
            return TryExecuteCommand(command, "say");
        }

        public static bool SendChatMessage(string message)
        {
            try
            {
                Svc.Framework.RunOnFrameworkThread(() =>
                {
                    Chat.Instance.SendMessage(message);
                    _logger.Debug($"Sent chat message: {message}");
                });
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send chat message");
                return false;
            }
        }

        public static bool SendShoutMessage(string message)
        {
            var command = $"/shout {message}";
            return TryExecuteCommand(command, "shout");
        }

        public static bool SendPartyMessage(string message)
        {
            var command = $"/party {message}";
            try
            {
                Svc.Framework.RunOnFrameworkThread(() =>
                {
                    Chat.Instance.SendMessage(command);
                    _logger.Debug($"Sent party message: {command}");
                });
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send party message");
                return false;
            }
        }

        public static bool SendTellMessage(string playerName, string message)
        {
            var command = $"/tell {playerName} {message}";
            try
            {
                Svc.Framework.RunOnFrameworkThread(() =>
                {
                    Chat.Instance.SendMessage(command);
                    _logger.Debug($"Sent tell message: '{command}' to: '{playerName}'");
                });
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send tell message");
                return false;
            }
        }

        public static bool SendFreeCompanyMessage(string message)
        {
            var command = $"/fc {message}";
            try
            {
                Svc.Framework.RunOnFrameworkThread(() =>
                {
                    Chat.Instance.SendMessage(command);
                    _logger.Debug($"Sent free company message: {command}");
                });
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send free company message");
                return false;
            }
        }

        private static bool TryExecuteCommand(string command, string typeLabel)
        {
            try
            {
                Svc.Framework.RunOnFrameworkThread(() =>
                {
                    Chat.Instance.ExecuteCommand(command);
                    _logger.Debug($"Sent {typeLabel} message: {command}");
                });
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to send {typeLabel} message");
                return false;
            }
        }
    }
}
