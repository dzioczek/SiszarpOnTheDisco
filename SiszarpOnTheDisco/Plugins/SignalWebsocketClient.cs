using System;
using System.Net.WebSockets;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Interactions;
using Websocket.Client;
using Serilog;
using SiszarpOnTheDisco.Signal;

namespace SiszarpOnTheDisco.Plugins;

public class SignalWebsocketClient
{
    private static readonly ManualResetEvent ExitEvent = new(false);

    private readonly ILogger _logger;
    private readonly SignalService _signalService;

    public SignalWebsocketClient(ILogger logger, SignalService signalService)
    {
        _logger = logger;
        _signalService = signalService;
    }

    public async void RunClient()
    {
        AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
        AssemblyLoadContext.Default.Unloading += DefaultOnUnloading;
        Console.CancelKeyPress += ConsoleOnCancelKeyPress;

        _logger.Debug("|=======================|");
        _logger.Debug("|    WEBSOCKET CLIENT   |");
        _logger.Debug("|=======================|");

        _logger.Debug("====================================");
        _logger.Debug("              STARTING              ");
        _logger.Debug("====================================");


        var factory = new Func<ClientWebSocket>(() =>
        {
            var client = new ClientWebSocket
            {
                Options =
                {
                    KeepAliveInterval = TimeSpan.FromSeconds(5),
                    // Proxy = ...
                    // ClientCertificates = ...
                }
            };
            //client.Options.SetRequestHeader("Origin", "xxx");
            return client;
        });
        string envAddress = Environment.GetEnvironmentVariable("SIGNAL_API_ADDRESS");
        var url = new Uri($"ws://{envAddress}/v1/receive/+48451165331");

        _signalService.ApiAddress = envAddress;

        using (IWebsocketClient client = new WebsocketClient(url))
        {
            client.Name = "siszarp";
            client.ReconnectTimeout = TimeSpan.FromSeconds(300);
            client.ErrorReconnectTimeout = TimeSpan.FromSeconds(30);
            client.ReconnectionHappened.Subscribe(info =>
            {
                _logger.Information("Reconnection happened, type: {type}, url: {url}", info.Type, client.Url);
            });
            client.DisconnectionHappened.Subscribe(info =>
                _logger.Warning("Disconnection happened, type: {Type} info: {Message}", info.Type,
                    info.Exception.Message));

            client.MessageReceived.Subscribe(msg =>
            {
                _signalService.ReadAndRespond(msg);
                _logger.Information("Message received: {message}", msg);
            });

            _logger.Information("Starting...");
            client.Start().Wait();
            _logger.Information("Started.");

            await Task.Run(() => StartSendingPing(client));

            ExitEvent.WaitOne();
        }

        _logger.Debug("====================================");
        _logger.Debug("              STOPPING              ");
        _logger.Debug("====================================");
    }

    private static async Task StartSendingPing(IWebsocketClient client)
    {
        while (true)
        {
            await Task.Delay(1000);

            if (!client.IsRunning)
                continue;

            client.Send("ping");
        }
    }

    private static void CurrentDomainOnProcessExit(object sender, EventArgs eventArgs)
    {
        ExitEvent.Set();
    }

    private static void DefaultOnUnloading(AssemblyLoadContext assemblyLoadContext)
    {
        ExitEvent.Set();
    }

    private static void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        ExitEvent.Set();
    }
}