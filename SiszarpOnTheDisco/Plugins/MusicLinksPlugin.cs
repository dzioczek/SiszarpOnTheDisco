using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Serilog;
using SiszarpOnTheDisco.Models;
using SiszarpOnTheDisco.Models.MusicLinks;

namespace SiszarpOnTheDisco.Plugins;

public class MusicLinksPlugin
{
    private const int MinutesToVote = 15;
    private readonly ApplicationDbContext _context;
    private readonly ILogger _logger;

    private int _linkToVoteId;
    private DateTime _voteExpireTime;

    public MusicLinksPlugin(ApplicationDbContext musicLinksContext, ILogger logger)
    {
        _context = musicLinksContext;
        _logger = logger;
    }

    public string AddNewLink(string url)
    {
        List<MusicLink> linkList = _context.MusicLinks.ToList();

        if (linkList.Exists(x => x.url.Equals(url.Trim()))) return "Było! Wysil sie trochę...";

        string title = GetTitle(url);

        MusicLink musicLink = new()
        {
            url = url,
            Created = DateTime.UtcNow,
            Title = title
        };

        _context.Add(musicLink);
        _context.SaveChanges();
        return $"Set \"{title}\" dodany!";
    }

    public MusicLink GetRandomLink()
    {
        List<MusicLink> linkList = _context.MusicLinks.ToList();
        if (linkList.Count == 0) return null;

        int i = new Random().Next(1, linkList.Count);

        MusicLink musicLink = linkList.FirstOrDefault(x => x.ID.Equals(i));

        if (musicLink != null)
        {
            musicLink.Apperrances += 1;
            _context.SaveChanges();
            _linkToVoteId = musicLink.ID;
            _voteExpireTime = DateTime.UtcNow.AddMinutes(MinutesToVote);
        }
        else
        {
            return GetRandomLink();
        }

        return musicLink;
    }

    private Regex SearchRegex(string searchTerm)
    {
        string[] arr = searchTerm.Split(" ");
        //^(?=.* fatboy).*$

        StringBuilder stringBuilder = new();

        stringBuilder.Append('^');
        foreach (string item in arr) stringBuilder.Append($"(?=.*{item})");

        stringBuilder.Append(".*$");

        return new Regex(stringBuilder.ToString(),
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);
    }

    public string SearchSet(string searchTerm)
    {
        Regex regex = SearchRegex(searchTerm);


        Func<MusicLink, bool> predicate = x =>
            regex.Match(x.Title).Success || (x.Tags != null ? regex.Match(x.Tags).Success : false);

        List<MusicLink> linkList = _context.MusicLinks.AsEnumerable()
            .Where(x => x.Title != null)
            .Where(predicate)
            .ToList();

        if (linkList.Count == 0) return "Nic nie znalazłem :(";

        StringBuilder sb = new();
        foreach (MusicLink link in linkList)
        {
            string tags = link.Tags != null ? link.Tags.Replace(";", " ").TrimEnd() : string.Empty;
            sb.AppendLine($"{link.Title} - {link.url} [{tags}]");
        }

        return sb.ToString();
    }

    public bool Vote(bool isPositive)
    {
        if (DateTime.UtcNow.CompareTo(_voteExpireTime) < 1)
        {
            MusicLink musicLink = _context.MusicLinks.AsQueryable().FirstOrDefault(m => m.ID.Equals(_linkToVoteId));

            if (musicLink != null)
            {
                musicLink.Votes += 1;
                if (isPositive)
                    musicLink.Yay += 1;
                else
                    musicLink.Meh += 1;
            }

            _context.SaveChanges();
            return true;
        }

        return false;
    }

    private static string GetTitle(string url)
    {
        HtmlWeb web = new();
        HtmlDocument htmlDoc = web.Load(url);
        HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode("//head/title");
        return node.InnerText;
    }

    public string PopulateLinks()
    {
        try
        {
            foreach (MusicLink link in _context.MusicLinks.ToList())
            {
                string title = string.Empty;
                try
                {
                    title = GetTitle(link.url);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("404 (Not Found)")) title = ex.Message;
                }

                Thread.Sleep(1000);
                link.Title = title;
                _context.SaveChanges();
            }


            return "done;";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    public string TagSet(string input)
    {
        List<string> args = new Regex(@"\s+").Split(input).ToList();

        try
        {
            string url = args.First().Trim();

            MusicLink link = _context.MusicLinks.AsQueryable().FirstOrDefault(x => x.url.Equals(url));
            args.Remove(args.First());

            if (link == null) return "link nie istnieje... chyba...";

            List<string> existingTags = new();
            if (link.Tags != null) existingTags = link.Tags.Split(";").ToList();

            foreach (string tag in args)
                if (!existingTags.Contains(tag.Trim()))
                    link.Tags += $"{tag.Trim()};";

            _context.SaveChanges();
            return "otagowane";
        }
        catch (Exception ex)
        {
            return ex.StackTrace;
        }
    }

    public async Task<string> DeleteSet(string link)
    {
        try
        {
            MusicLink musicLink = _context.MusicLinks.FirstOrDefault(x => x.url == link.Trim());
            if (musicLink == null) return "Nie znalazłem takiego seta.";

            _context.MusicLinks.Remove(musicLink);
            await _context.SaveChangesAsync();
            return "Usunięte!";
        }
        catch (Exception ex)
        {
            _logger.Error("Issues while deleting set. {Message}", ex.Message);
            return "Cos poszło nie tak. Sprawdź se loga.";
        }
    }
}