using System;
using Discord.Commands;

namespace SiszarpOnTheDisco.Models.Lawn;

public class LawnEvent : IComparable
{
    public int Id { get; set; }
    public LawnEventTypes EventType { get; set; }
    public string Gardner { get; set; }
    public DateOnly EventDate { get; set; }
    public  string? Comment { get; set; }

    public override string ToString()
    {
        return $"{EventType.Name} - {EventDate:dd/MM/yyy} ({(DateTime.Today - EventDate.ToDateTime(TimeOnly.MinValue)).TotalDays} dni temu) {Comment}";
    }   

    public int CompareTo(object obj)
    {
        LawnEvent other = (LawnEvent)obj;
        return this.EventType.CompareTo(other.EventType);
    }
}