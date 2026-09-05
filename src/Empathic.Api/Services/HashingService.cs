using System.Security.Cryptography;
using System.Text;

namespace Empathic.Api.Services;

public sealed class HashingService
{
    public string Sha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
