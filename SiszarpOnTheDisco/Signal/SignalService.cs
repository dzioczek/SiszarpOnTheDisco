using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using SiszarpOnTheDisco.Plugins;
using SiszarpOnTheDisco.Signal.Models.ApiModels;
using SiszarpOnTheDisco.Signal.Models.IncomingMessages;

namespace SiszarpOnTheDisco.Signal;

public class SignalService
{
    private readonly ILogger _logger;
    private readonly HomeAssistantPlugin _homeAssistantPlugin;
    private readonly MusicLinksPlugin _musicLinksPlugin;
    
    public string ApiAddress { get; set; }
    
    public SignalService(ILogger logger, HomeAssistantPlugin homeAssistantPlugin, MusicLinksPlugin musicLinksPlugin)
    {
        _logger = logger;
        _homeAssistantPlugin = homeAssistantPlugin;
        _musicLinksPlugin = musicLinksPlugin; 
    }

    public async Task<string> ReadAndRespond(string data, CancellationToken cancellationToken)
    {

        RpcResponse<SignalMessage, object> rpcResponse = JsonSerializer.Deserialize<RpcResponse<SignalMessage,object>>(data);

        if (rpcResponse.Error is not null) _logger.Error(rpcResponse.Error.Message);

        SignalMessage? message = rpcResponse?.Params;

        if (message?.Envelope.DataMessage != null)
        {
            string messageText = message.Envelope.DataMessage.Message!;

            if (!messageText.FirstOrDefault().Equals('!')) return string.Empty;

            messageText = messageText.Remove(0, 1);

            RpcResponse<Message,object> response = new()
            {
                Method = "send",
                Params = GetResponse(messageText)
            };

            if (message.Envelope.DataMessage.GroupInfo != null)
            {
                response.Params.GroupId = message.Envelope.DataMessage.GroupInfo.GroupId;
            }
            else
            {
                response.Params.Recipients = [message.Envelope.Source];
            }

            JsonSerializerOptions options = new ()
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            string serialized = JsonSerializer.Serialize(response, options);
            _logger.Debug(serialized);
            return serialized;
        }
        return string.Empty;
    }

    private Message GetResponse(string commandMessage)
    {
        Regex r = new Regex(@"\s{2,}");
        
        commandMessage = r.Replace(commandMessage, @" ");    
        string[] arr = commandMessage.Split(" ");

        string command = arr[0];

        Message m = new(); 
        
        m.Text =  command switch
        {
            "out" => _homeAssistantPlugin.GetOutsideWeather(),
            "dajseta" => _musicLinksPlugin.GetRandomLink().url,
            "dodajseta" => _musicLinksPlugin.AddNewLink(arr[1]),
            "dejkrisa" => "A masz se posłuchaj... https://www.youtube.com/watch?v=N1KbTCS-Sz8:", 
            "szukajseta" => _musicLinksPlugin.SearchSet(string.Join(" ", arr.Skip(1))),
            "dajamber" => _musicLinksPlugin.SearchSet("amber"),
            "usunseta" => _musicLinksPlugin.DeleteSet(arr[1]).Result,
            "yay" => _musicLinksPlugin.Vote(true).ToString(),
            "meh" => _musicLinksPlugin.Vote(false).ToString(),
            "tagujseta" => _musicLinksPlugin.TagSet(arr[1], string.Join(" ", arr.Skip(2))),
            "cam" => "Nice pics! ;]",
            "temp" => _homeAssistantPlugin.GetTemperatures(),
            "power" => _homeAssistantPlugin.GetPowerStatus().Result,
            _ => $"Command {command} not found..."
        };

        m.Attachments = command switch
        {
            "cam" => _homeAssistantPlugin.GetLocalPicturePathAsync().Result,
            "power" => [_homeAssistantPlugin.GetPowerChart().Result.FullName],
            _ => m.Attachments
        };

        return m; 
    }

}