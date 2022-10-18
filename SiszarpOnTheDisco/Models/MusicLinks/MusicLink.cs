using System;

namespace SiszarpOnTheDisco.Models.MusicLinks;

public class MusicLink
{
    public int ID { get; set; }
    public DateTime Created { get; set; }
    public string url { get; set; }
    public int Votes { get; set; }
    public int Yay { get; set; }
    public int Meh { get; set; }
    public int Apperrances { get; set; }
    public string Title { get; set; }
    public string Tags { get; set; }
    public string YoutubeChannel { get; set; }

    public string Stats => string.Format($"[yay: {Yay}, meh: {Meh}, votes: {Votes}, appearances: {Apperrances}]");

    public override string ToString()
    {
        return $"{url}  [yay: {Yay}, meh: {Meh}, votes: {Votes}, appearances: {Apperrances}]";
    }
}