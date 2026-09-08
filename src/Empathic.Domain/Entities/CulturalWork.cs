namespace Empathic.Domain.Entities;

public sealed record CulturalWork(
    Guid Id,
    Guid CreatorId,
    string Title,
    string WorkType,
    string Description,
    string ContentHash,
    string? ExternalUri,
    string RightsStatement,
    DateTimeOffset CreatedAtUtc,
    string ProvenanceStatus,
    string? BlockchainNetwork = null,
    string? TransactionHash = null);
