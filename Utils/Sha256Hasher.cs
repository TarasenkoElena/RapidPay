using System.Security.Cryptography;
using System.Text;

namespace RapidPay.Utils;

public interface IHasher
{
    string Compute(string value);
}

public class Sha256Hasher : IHasher, IDisposable
{
    private readonly SHA256 _sha256 = SHA256.Create();

    public string Compute(string value)
    {
        var inputBytes = Encoding.UTF8.GetBytes(value);
        var hashBytes = _sha256.ComputeHash(inputBytes);

        return Convert.ToBase64String(hashBytes);
    }

    public void Dispose()
    {
        _sha256.Dispose();
    }
}
