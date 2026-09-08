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

public sealed record DashboardSummary(int Creators, int Works, int AnchoredWorks, int PendingAnchoring);
