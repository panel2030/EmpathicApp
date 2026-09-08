using System.Security.Cryptography;
using System.Text;
using Empathic.Application.Abstractions;

namespace Empathic.Infrastructure.Services;

public sealed class Sha256HashingService : IHashingService
{
    public string Sha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

public sealed class DevelopmentBlockchainAnchorService : IBlockchainAnchorService
{
    public Task<BlockchainAnchorResult> AnchorAsync(
        string contentHash,
        Guid workId,
        Guid creatorId,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var anchoredAt = DateTimeOffset.UtcNow;
        var seed = $"{contentHash}:{workId:N}:{creatorId:N}:{anchoredAt:O}";
        var tx = "0x" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(seed))).ToLowerInvariant();
        return Task.FromResult(new BlockchainAnchorResult("development-ledger", tx, anchoredAt));
    }
}
