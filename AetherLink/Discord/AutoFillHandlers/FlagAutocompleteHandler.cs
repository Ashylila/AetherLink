using System;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Game.Text;
using Discord;
using Discord.Interactions;

namespace AetherLink.Discord.AutoFillHandlers;

public class FlagAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context, IAutocompleteInteraction interaction, IParameterInfo parameter, IServiceProvider services)
    {
        string userInput = interaction.Data.Current.Value.ToString() ?? string.Empty;

        var suggestions = Enum.GetNames<XivChatType>()
            .Where(x => x.StartsWith(userInput, StringComparison.OrdinalIgnoreCase))
            .Take(5)
            .Select(flag => new AutocompleteResult(flag, flag))
            .ToList();

        return AutocompletionResult.FromSuccess(suggestions);
    }
}
