using System.Net.Mail;
using Empathic.Application.Abstractions;
using Empathic.Application.Contracts;
using Empathic.Domain.Entities;

namespace Empathic.Application.Services;

public sealed class CreatorService(ICreatorRepository creators)
{
    public Task<IReadOnlyList<Creator>> GetAllAsync(CancellationToken ct = default) => creators.GetAllAsync(ct);
    public Task<Creator?> GetAsync(Guid id, CancellationToken ct = default) => creators.GetAsync(id, ct);

    public async Task<Creator> CreateAsync(CreateCreatorRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName))
            throw new ArgumentException("Display name is required.");

        var creator = new Creator(
            Guid.NewGuid(),
            request.DisplayName.Trim(),
            request.Institution?.Trim(),
            request.Country?.Trim(),
            request.Website?.Trim(),
            request.Roles?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray() ?? [],
            DateTimeOffset.UtcNow);

        await creators.AddAsync(creator, ct);
        return creator;
    }
}

public sealed class CulturalWorkService(
    ICreatorRepository creators,
    ICulturalWorkRepository works,
    IHashingService hashing)
{
    public Task<IReadOnlyList<CulturalWork>> GetAllAsync(CancellationToken ct = default) => works.GetAllAsync(ct);
    public Task<CulturalWork?> GetAsync(Guid id, CancellationToken ct = default) => works.GetAsync(id, ct);
    public Task<CulturalWork?> FindByHashAsync(string hash, CancellationToken ct = default) => works.FindByHashAsync(hash.Trim(), ct);

    public async Task<CulturalWork> CreateAsync(CreateWorkRequest request, CancellationToken ct = default)
    {
        if (await creators.GetAsync(request.CreatorId, ct) is null)
            throw new InvalidOperationException("Creator does not exist.");

        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.WorkType))
            throw new ArgumentException("Title and work type are required.");

        var material = string.IsNullOrWhiteSpace(request.ContentFingerprintSource)
            ? $"{request.CreatorId:N}|{request.Title.Trim()}|{request.WorkType.Trim()}|{request.Description?.Trim()}|{request.ExternalUri?.Trim()}"
            : request.ContentFingerprintSource.Trim();

        var hash = hashing.Sha256(material);
        if (await works.FindByHashAsync(hash, ct) is not null)
            throw new DuplicateContentException(hash);

        var work = new CulturalWork(
            Guid.NewGuid(), request.CreatorId, request.Title.Trim(), request.WorkType.Trim(),
            request.Description?.Trim() ?? string.Empty, hash, request.ExternalUri?.Trim(),
            string.IsNullOrWhiteSpace(request.RightsStatement) ? "All rights reserved" : request.RightsStatement.Trim(),
            DateTimeOffset.UtcNow, "registered-off-chain");

        await works.AddAsync(work, ct);
        return work;
    }
}

public sealed class MovementMemberService(IMovementMemberRepository members)
{
    private static readonly HashSet<string> AllowedInterestTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Supporter", "Artist or Writer", "Museum or Gallery", "University or Research",
        "Cultural Organisation", "Technology Partner", "Sponsor or Funder", "Other"
    };

    public async Task<MovementMember> JoinAsync(JoinMovementRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName))
            throw new ArgumentException("Your name is required.");
        if (string.IsNullOrWhiteSpace(request.Email) || !IsValidEmail(request.Email))
            throw new ArgumentException("A valid email address is required.");
        if (!request.ConsentToUpdates)
            throw new ArgumentException("Please confirm that we may store your details for this request.");

        var email = request.Email.Trim().ToLowerInvariant();
        if (await members.FindByEmailAsync(email, ct) is not null)
            throw new DuplicateMemberException();

        var interest = string.IsNullOrWhiteSpace(request.InterestType) ? "Supporter" : request.InterestType.Trim();
        if (!AllowedInterestTypes.Contains(interest)) interest = "Other";

        var member = new MovementMember(
            Guid.NewGuid(), request.DisplayName.Trim(), email, request.Country?.Trim(), interest,
            request.Message?.Trim(), true, DateTimeOffset.UtcNow);

        await members.AddAsync(member, ct);
        return member;
    }

    public async Task<MovementStats> GetStatsAsync(CancellationToken ct = default) =>
        new(await members.CountAsync(ct));

    private static bool IsValidEmail(string value)
    {
        try { return new MailAddress(value.Trim()).Address.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase); }
        catch { return false; }
    }
}

public sealed class ProvenanceService(ICulturalWorkRepository works, IBlockchainAnchorService blockchain)
{
    public async Task<CulturalWork?> AnchorAsync(Guid id, CancellationToken ct = default)
    {
        var work = await works.GetAsync(id, ct);
        if (work is null) return null;
        if (!string.IsNullOrWhiteSpace(work.TransactionHash)) return work;

        var result = await blockchain.AnchorAsync(work.ContentHash, work.Id, work.CreatorId, ct);
        var updated = work with
        {
            ProvenanceStatus = "anchored",
            BlockchainNetwork = result.Network,
            TransactionHash = result.TransactionHash
        };
        await works.UpdateAsync(updated, ct);
        return updated;
    }
}

public sealed class DashboardService(ICreatorRepository creators, ICulturalWorkRepository works)
{
    public async Task<DashboardSummary> GetAsync(CancellationToken ct = default)
    {
        var creatorList = await creators.GetAllAsync(ct);
        var workList = await works.GetAllAsync(ct);
        return new DashboardSummary(
            creatorList.Count,
            workList.Count,
            workList.Count(x => x.ProvenanceStatus == "anchored"),
            workList.Count(x => x.ProvenanceStatus != "anchored"));
    }
}

public sealed class DuplicateContentException(string contentHash)
    : Exception("This content fingerprint is already registered.")
{
    public string ContentHash { get; } = contentHash;
}

public sealed class DuplicateMemberException() : Exception("This email is already registered with the movement.");
