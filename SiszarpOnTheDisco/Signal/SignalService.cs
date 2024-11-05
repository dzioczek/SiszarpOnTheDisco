using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Serilog;
using SiszarpOnTheDisco.Plugins;
using SiszarpOnTheDisco.Signal.Models.ApiModels;
using SiszarpOnTheDisco.Signal.Models.IncomingMessages;
using Websocket.Client;
using static System.Text.Json.JsonSerializer;
using Group = SiszarpOnTheDisco.Signal.Models.ApiResponses.Group;

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
    
    public async Task ReadAndRespond(ResponseMessage responseMessage)
    {
        SignalMessage? message = Deserialize<SignalMessage>(responseMessage.Text!);

        if (message?.Envelope.DataMessage != null)
        {
            string messageText = message.Envelope.DataMessage.Message!;

            if (messageText.FirstOrDefault().Equals('!'))
            {
                messageText = messageText.Remove(0, 1);
                Message res = GetResponse(messageText); 
                
                if (res != null)
                {
                    using HttpClient httpClient = new HttpClient();
                    httpClient.BaseAddress = new Uri($"http://{ApiAddress}/");

                    string replyTo = message.Envelope.SourceNumber;

                    if (message.Envelope.DataMessage.GroupInfo != null)
                    {
                        string localId = message.Envelope.DataMessage.GroupInfo.GroupId!;
                        _logger.Debug(localId);
                        List<Group> groups = httpClient.GetFromJsonAsync<List<Group>>("v1/groups/+48451165331").Result;
                        if (groups == null) return;
                        Group? targetGroup = groups.FirstOrDefault(g => g.InternalId!.Equals(localId));
                        _logger.Debug(targetGroup!.Id);
                        replyTo = targetGroup!.Id!;
                    }

                    _logger.Debug(replyTo);
                    
                    if (replyTo != null)
                    {
                        res.Recipients = [replyTo];

                        string serialized = Serialize(res);
                        _logger.Debug(serialized);

                        HttpResponseMessage response =
                            await httpClient.PostAsync("v2/send", new StringContent(serialized));

                        _logger.Debug(response.StatusCode.ToString());
                    }
                }
            }
        }
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
            "cam" => _homeAssistantPlugin.GetLocalPicturePathAsync().Result.FirstOrDefault(),
            _ => $"Command {command} not found..."
        };
        
        if (command == "cam")
        {
            byte[] data = File.ReadAllBytes(m.Text);
            m.Attachments.Add(Convert.ToBase64String(data));
        }
        return m; 
    }
    
}