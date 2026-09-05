namespace Empathic.Api.Services;

public interface IBlockchainAnchorService
{
    Task<BlockchainAnchorResult> AnchorAsync(string contentHash, Guid workId, Guid creatorId, CancellationToken cancellationToken = default);
}

public sealed record BlockchainAnchorResult(string Network, string TransactionHash, DateTimeOffset AnchoredAtUtc);

/// <summary>
/// Phase-1 development adapter. It gives the application the same contract that a real
/// Polygon/Ethereum adapter will implement, without introducing wallet/private-key risk
/// before the provenance workflow is approved.
/// </summary>
public sealed class DevelopmentBlockchainAnchorService : IBlockchainAnchorService
{
    public Task<BlockchainAnchorResult> AnchorAsync(string contentHash, Guid workId, Guid creatorId, CancellationToken cancellationToken = default)
    {
        var seed = $"{contentHash}:{workId:N}:{creatorId:N}:{DateTimeOffset.UtcNow:O}";
        var tx = "0x" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(seed))).ToLowerInvariant();
        return Task.FromResult(new BlockchainAnchorResult("development-ledger", tx, DateTimeOffset.UtcNow));
    }
}
