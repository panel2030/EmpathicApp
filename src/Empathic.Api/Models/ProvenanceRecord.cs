namespace Empathic.Api.Models;

public sealed record ProvenanceRecord(
    Guid WorkId,
    string ContentHash,
    Guid CreatorId,
    DateTimeOffset RegisteredAtUtc,
    string Status,
    string? Network,
    string? TransactionHash);
