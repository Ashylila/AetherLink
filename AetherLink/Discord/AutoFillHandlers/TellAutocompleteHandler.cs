using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace AetherLink.Discord.AutoFillHandlers;

public class TellAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,IAutocompleteInteraction Interaction,IParameterInfo parameter, IServiceProvider services)
    {
        string userInput = Interaction.Data.Current.Value.ToString();

        var suggestions = DiscordHandler.chatMessages.Where(message => message.Sender.StartsWith(userInput, StringComparison.OrdinalIgnoreCase)).Select(message => new AutocompleteResult(message.Sender, message.Sender)).Take(5).ToList();
        return AutocompletionResult.FromSuccess(suggestions);
    }
}
