namespace Empathic.Domain.Entities;

public sealed record MovementMember(
    Guid Id,
    string DisplayName,
    string Email,
    string? Country,
    string InterestType,
    string? Message,
    bool ConsentToUpdates,
    DateTimeOffset CreatedAtUtc);
