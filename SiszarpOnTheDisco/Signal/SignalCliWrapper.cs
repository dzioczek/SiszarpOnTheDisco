using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using SiszarpOnTheDisco.Signal.Models.IncomingMessages;

namespace SiszarpOnTheDisco.Signal;

public class SignalCliWrapper(ILogger logger, SignalService service, SignalUpdateService updateService)
{
    private readonly Process _signal = new();
    private string _error;
    private List<Group> _groups = [];
    private string? Signal { get; set; }

    public async void StartAsync(object obj)
    {
        if (obj == null) return;
        CancellationToken token = (CancellationToken)obj;

        await updateService.CheckUpdates(token);
        Signal = updateService.ClientPath;

        try
        {
            ProcessStartInfo processStartInfo = new()
            {
                FileName =  Signal,
                ArgumentList = { "-a", "+48451165331", "jsonRpc", "--config /signal/config" },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _signal.StartInfo = processStartInfo;
            _signal.OutputDataReceived += (sender, e) => ReadMessageAsync(sender, e, token);
            _signal.Exited += ProcessExited;
            _signal.EnableRaisingEvents = true;

            _signal.Start();
            _signal.BeginOutputReadLine();

            await GetGroupsAsync(token);

            _error = await _signal.StandardError.ReadToEndAsync(token);

            await _signal.WaitForExitAsync(token);
            _signal.Close();
        }
        catch (Exception e)
        {
            logger.Error(e, "Failed to start signal");
        }
    }

    private async void ReadMessageAsync(object sender, DataReceivedEventArgs e, CancellationToken cancellationToken)
    {
        try
        {
            if (e.Data == null) return;
            logger.Information("Received signal: {Data}", e.Data);

            string response = await service.ReadAndRespond(e.Data, cancellationToken);

            if (response.Length == 0) return;
            StreamWriter writer = _signal.StandardInput;
            await writer.WriteLineAsync(response);
        }
        catch (Exception exception)
        {
            logger.Error(exception, "Failed to read signal");
        }
    }

    private async Task GetGroupsAsync(CancellationToken cancellationToken)
    {
        StreamWriter writer = _signal.StandardInput;
        RpcResponse<object,object> listGroups = new()
        {
            Method = "listGroups"
        };

        await writer.WriteLineAsync(JsonSerializer.Serialize(listGroups));
    }

    public void CloseSignal()
    {
        logger.Information("Closing signal");
        _signal.Close();
        logger.Information("Signal closed");
    }

    private async void ProcessExited(object sender, EventArgs e)
    {
        logger.Information("Signal exit code: {S}", _signal.ExitCode.ToString());
    }
}