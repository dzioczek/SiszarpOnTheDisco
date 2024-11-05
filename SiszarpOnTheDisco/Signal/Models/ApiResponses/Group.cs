using System.Text.Json.Serialization;

namespace SiszarpOnTheDisco.Signal.Models.ApiResponses;

public class Group
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("internal_id")] public string? InternalId { get; set; }
    [JsonPropertyName("members")] public string[]? Members { get; set; }
    [JsonPropertyName("blocked")] public bool Blocked { get; set; }
    [JsonPropertyName("pending_invites")] public string[]? PendingInvites { get; set; }
    [JsonPropertyName("pending_requests")] public string[]? PendingRequests { get; set; }
    [JsonPropertyName("invite_link")] public string? InviteLink { get; set; }
    [JsonPropertyName("admins")] public string[]? Admins { get; set; }
}