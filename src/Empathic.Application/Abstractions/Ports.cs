using Empathic.Domain.Entities;

namespace Empathic.Application.Abstractions;

public interface ICreatorRepository
{
    Task<IReadOnlyList<Creator>> GetAllAsync(CancellationToken ct = default);
    Task<Creator?> GetAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Creator creator, CancellationToken ct = default);
}

public interface ICulturalWorkRepository
{
    Task<IReadOnlyList<CulturalWork>> GetAllAsync(CancellationToken ct = default);
    Task<CulturalWork?> GetAsync(Guid id, CancellationToken ct = default);
    Task<CulturalWork?> FindByHashAsync(string hash, CancellationToken ct = default);
    Task AddAsync(CulturalWork work, CancellationToken ct = default);
    Task UpdateAsync(CulturalWork work, CancellationToken ct = default);
}

public interface IHashingService
{
    string Sha256(string value);
}

public interface IBlockchainAnchorService
{
    Task<BlockchainAnchorResult> AnchorAsync(string contentHash, Guid workId, Guid creatorId, CancellationToken ct = default);
}

public sealed record BlockchainAnchorResult(string Network, string TransactionHash, DateTimeOffset AnchoredAtUtc);
