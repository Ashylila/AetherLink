using System.Threading.Tasks;
using AetherLink.Models;
using Discord.WebSocket;
using System.Linq;
using AetherLink.Utility;
using AetherLink;
using Dalamud.Game.Text;
using AetherLink.DalamudServices;
using Discord;
using System.Collections.Generic;

namespace AetherLink.Discord.SlashCommands;
public class RemoveChatFlagCommand : ICommand
{
    public string Name => "removechatflag";
    public string Description => "Remove a chat flag.";
    public List<CommandOption> Options => new List<CommandOption>
    {
        new CommandOption
        {
            Type = ApplicationCommandOptionType.String,
            Name = "flag",
            Description = "The chat flag to remove.",
            IsRequired = true,
            IsAutoFill = true
    }};

    public async Task Execute(SocketInteraction interaction)
    {
        if (interaction is SocketSlashCommand command)
        {
            var config = Svc.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            var flagToRemove = command.Data.Options.FirstOrDefault(x => x.Name == "flag")?.Value as string;
            if (!EnumHelper.IsValidEnumMember<XivChatType>(flagToRemove) || (EnumHelper.TryConvertToEnum<XivChatType>(flagToRemove, out var result) && !config.ChatTypes.Contains(result)))
            {
                await interaction.RespondAsync("Invalid flag or it is already inactive.", ephemeral: true);
                await Task.Delay(5000);
                await interaction.DeleteOriginalResponseAsync();
                return;
            }
            Svc.Log.Debug($"Flag: {flagToRemove}");
            config.ChatTypes.Remove(result);
            config.Save();
            await interaction.RespondAsync($"Flag {flagToRemove} has been removed", ephemeral: true);
            await Task.Delay(5000);
            await interaction.DeleteOriginalResponseAsync();
            return;
        }
    }
}