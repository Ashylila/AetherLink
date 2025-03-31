using System.Threading.Tasks;
using AetherLink.Models;
using Discord.WebSocket;
using System.Linq;
using AetherLink.Utility;
using Dalamud.Game.Text;
using AetherLink.DalamudServices;
using AetherLink;
using Discord;
using System.Collections.Generic;

namespace AetherLink.Discord.SlashCommands;
public class AddChatFlagCommand : ICommand
{
    public string Name => "addchatflag";
    public string Description => "Add a chat flag.";
    public List<CommandOption> Options => new List<CommandOption>
    {
        new CommandOption
        {
            Type = ApplicationCommandOptionType.String,
            Name = "flag",
            Description = "The chat flag to add.",
            IsRequired = true,
            IsAutoFill = true
    }};

    public async Task Execute(SocketInteraction interaction)
    {
        if (interaction is SocketSlashCommand command)
        {
            var config = Plugin.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            var flag = command.Data.Options.FirstOrDefault(x => x.Name == "flag")?.Value as string;
            if (!EnumHelper.IsValidEnumMember<XivChatType>(flag) || (EnumHelper.TryConvertToEnum<XivChatType>(flag, out var addresult) && config.ChatTypes.Contains(addresult)))
            {
                await interaction.RespondAsync("Invalid flag or it is already active.", ephemeral: true);
                await Task.Delay(5000);
                await interaction.DeleteOriginalResponseAsync();
                return;
            }
            config.ChatTypes.Add(addresult);
            config.Save();
            await interaction.RespondAsync($"Flag {flag} has been added", ephemeral: true);
            await Task.Delay(5000);
            await interaction.DeleteOriginalResponseAsync();
            return;
        }
    }
}
