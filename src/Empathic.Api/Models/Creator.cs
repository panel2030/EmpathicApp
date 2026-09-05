namespace Empathic.Api.Models;

public sealed record Creator(
    Guid Id,
    string DisplayName,
    string? Institution,
    string? Country,
    string? Website,
    string[] Roles,
    DateTimeOffset CreatedAtUtc,
    bool IsVerified = false);
