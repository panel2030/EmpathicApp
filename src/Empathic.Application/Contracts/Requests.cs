namespace Empathic.Application.Contracts;

public sealed record CreateCreatorRequest(
    string DisplayName,
    string? Institution,
    string? Country,
    string? Website,
    string[]? Roles);

public sealed record CreateWorkRequest(
    Guid CreatorId,
    string Title,
    string WorkType,
    string? Description,
    string? ExternalUri,
    string? RightsStatement,
    string? ContentFingerprintSource);

public sealed record JoinMovementRequest(
    string DisplayName,
    string Email,
    string? Country,
    string InterestType,
    string? Message,
    bool ConsentToUpdates);

public sealed record DashboardSummary(int Creators, int Works, int AnchoredWorks, int PendingAnchoring);
public sealed record MovementStats(int Members);
