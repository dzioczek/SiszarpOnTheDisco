using System;

namespace SiszarpOnTheDisco.Models;

public class EndUser
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public required string Username { get; set; }

    public string? SignalUsername { get; set; }

    public string? DiscordUsername { get; set; }

    public bool IsAdmin { get; set; } =  false;

}