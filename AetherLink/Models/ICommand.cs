
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace AetherLink.Models;

public interface ICommand
{
    public string Name { get; }
    string Description { get; }
    List<CommandOption> Options { get; }
    public Task Execute(SocketInteraction interaction);
}