using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using SiszarpOnTheDisco.CommandModules;
using SiszarpOnTheDisco.Models;
using SiszarpOnTheDisco.Plugins;
using SiszarpOnTheDisco.Signal;

namespace SiszarpOnTheDisco;

internal class Program
{
    private DiscordSocketClient _client;
    private CommandHandler _commandHandler;
    private CommandService _commands;
    private IServiceProvider _services;
    private ulong _guildId;

    private InteractionService _interactionService;

    private CancellationTokenSource _cts;

    private static void Main(string[] args)
    {
        new Program().MainAsync().GetAwaiter().GetResult();
    }

    public async Task MainAsync()
    {
        _cts = new CancellationTokenSource();
        CancellationToken cancellationToken = _cts.Token;

        Console.CancelKeyPress += QuitSignal;
        Console.CancelKeyPress += Quit;

        var config = new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.All,
        };

        _client = new DiscordSocketClient(config);
        _client.Log += Log;

        _interactionService = new InteractionService(_client.Rest);

        _client.Ready += ClientReady;

        _guildId = ulong.Parse(Environment.GetEnvironmentVariable("GUILD_ID"));
        string token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");

        _commands = new CommandService();
        _commands.Log += Log;
        _services = ConfigureServices();
        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        _commandHandler = new CommandHandler(_client, _commands, _services);
        await _commandHandler.InstallCommandAsync();

        _client.InteractionCreated += async (x) =>
        {
            SocketInteractionContext ctx = new(_client, x);
            await _interactionService.ExecuteCommandAsync(ctx, _services);
        };


        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        SignalCliWrapper signalClient = _services.GetRequiredService<SignalCliWrapper>();
        ThreadPool.QueueUserWorkItem(signalClient.StartAsync, cancellationToken);

        await Task.Delay(-1);
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private static IServiceProvider ConfigureServices()
    {
        IServiceCollection map = new ServiceCollection()
            // Repeat this for all the service classes
            // and other dependencies that your commands might need.
            .AddSingleton(GetLogger())
            .AddSingleton<HomeAssistantPlugin>()
            .AddSingleton<HomeAssistantCommands>()
            .AddSingleton<MusicLinksPlugin>()
            .AddTransient<MusicCommands>()
            .AddSingleton<GeneralCommands>()
            .AddSingleton<LawnPlugin>()
            .AddSingleton<ChallengeCommands>()
            .AddTransient<LawnCommands>()
            .AddTransient<TodoCommands>()
            .AddTransient<HelpCommands>()
            .AddSingleton<SignalService>()
            .AddSingleton<SignalCliWrapper>()
            .AddSingleton<SignalUpdateService>()
            .AddDbContext<ApplicationDbContext>(
                options => options.UseNpgsql(GetConnectionString()));


        // When all your required services are in the collection, build the container.
        // Tip: There's an overload taking in a 'validateScopes' bool to make sure
        // you haven't made any mistakes in your dependency graph.
        return map.BuildServiceProvider();
    }

    private static ILogger GetLogger()
    {
        return new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            //.WriteTo.File("log.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
            .CreateLogger();
    }

    private void Quit(object sender, ConsoleCancelEventArgs args)
    {
        _client.StopAsync();
        Environment.Exit(0);
    }

    private void QuitSignal(object sender, ConsoleCancelEventArgs consoleCancelEventArgs)
    {
        SignalCliWrapper signalClient = _services.GetRequiredService<SignalCliWrapper>();
        signalClient.CloseSignal();
    }

    // TODO: refactor...
    private static string GetConnectionString()
    {
        return $"Server={Environment.GetEnvironmentVariable("PG_HOST")};Port={Environment.GetEnvironmentVariable("PG_PORT")};Database={Environment.GetEnvironmentVariable("PG_DATABASE")};Username={Environment.GetEnvironmentVariable("PG_USERNAME")};Password={Environment.GetEnvironmentVariable("PG_PASSWORD")}";
    }

    public async Task ClientReady()
    {
        try
        {
            await _interactionService.RegisterCommandsToGuildAsync(_guildId);
            
        }
        catch (HttpException ex)
        {
            var json = JsonConvert.SerializeObject(ex, Formatting.Indented);
            Console.WriteLine(json);
        }

    }

}