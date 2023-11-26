using System.Buffers;
using System.Security.Cryptography;

namespace RapidPay.Utils;

public interface ISaltGenerator
{
    string GenerateSalt();
}

public class SaltGenerator : ISaltGenerator, IDisposable
{
    private const int SaltSize = 24;
    private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    public string GenerateSalt()
    {
        using var memory = MemoryPool<byte>.Shared.Rent(SaltSize);
        _rng.GetBytes(memory.Memory.Span);
        return Convert.ToBase64String(memory.Memory.Span);
    }

    public void Dispose()
    {
        _rng.Dispose();
    }
}
