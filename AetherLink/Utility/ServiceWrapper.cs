using System;
using AetherLink.DalamudServices;
using AetherLink.Discord;
using AetherLink.Windows;
using Dalamud.Plugin;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AetherLink.Utility;

public class ServiceWrapper
{
    public static IServiceProvider Services { get; private set; } = null!;

    public static void Init(IDalamudPluginInterface pluginInterface, Plugin plugin)
    {
        Svc.Init(pluginInterface);
        
        var serviceCollection = new ServiceCollection();
        
        serviceCollection.AddSingleton(plugin);
        serviceCollection.AddSingleton(plugin.Configuration);
        
        serviceCollection.AddSingleton(Svc.Log);
        serviceCollection.AddSingleton(Svc.Chat);
        serviceCollection.AddSingleton(Svc.ClientState);
        serviceCollection.AddSingleton(Svc.Framework);
        serviceCollection.AddSingleton(Svc.Data);
        serviceCollection.AddSingleton(Svc.PluginInterface);

        serviceCollection.AddSingleton<ChatHandler>();
        serviceCollection.AddSingleton<DiscordHandler>();
        serviceCollection.AddSingleton<CommandHandler>();
        
        serviceCollection.AddSingleton<DiscordSocketClient>();
        serviceCollection.AddSingleton<InteractionService>(provider =>
        {
            var client = provider.GetRequiredService<DiscordSocketClient>();
            return new InteractionService(client);
        });
        serviceCollection.AddSingleton(new DiscordSocketConfig()
        {
                MessageCacheSize = 20,
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMessages | GatewayIntents.GuildWebhooks | GatewayIntents.MessageContent,
        });
        
        serviceCollection.AddSingleton<MainWindow>();
        serviceCollection.AddSingleton<ConfigWindow>();
        serviceCollection.AddSingleton<LogWindow>();
        
        Services = serviceCollection.BuildServiceProvider();
    }
    public static T Get<T>() where T : class
    {
        return Services.GetRequiredService<T>();
    }
    public static Object Get(Type type)
    {
        return Services.GetRequiredService(type);
    }
}
