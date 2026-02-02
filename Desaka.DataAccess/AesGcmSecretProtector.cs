using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace Desaka.DataAccess;

public sealed class AesGcmSecretProtector : ISecretProtector
{
    private readonly byte[] _key;

    public AesGcmSecretProtector(IOptions<SecretProtectorOptions> options)
    {
        var keyBase64 = options.Value.Base64Key;
        if (string.IsNullOrWhiteSpace(keyBase64))
        {
            throw new InvalidOperationException("Secrets:Base64Key must be configured.");
        }

        _key = Convert.FromBase64String(keyBase64);
        if (_key.Length is not (16 or 24 or 32))
        {
            throw new InvalidOperationException("Secrets:Base64Key must be 128/192/256-bit length.");
        }
    }

    public string Protect(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
        {
            return string.Empty;
        }

        var nonce = RandomNumberGenerator.GetBytes(12);
        var plaintextBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
        var cipher = new byte[plaintextBytes.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(_key);
        aes.Encrypt(nonce, plaintextBytes, cipher, tag);

        var payload = new byte[nonce.Length + tag.Length + cipher.Length];
        Buffer.BlockCopy(nonce, 0, payload, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, payload, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipher, 0, payload, nonce.Length + tag.Length, cipher.Length);

        return Convert.ToBase64String(payload);
    }

    public string Unprotect(string protectedValue)
    {
        if (string.IsNullOrEmpty(protectedValue))
        {
            return string.Empty;
        }

        var payload = Convert.FromBase64String(protectedValue);
        if (payload.Length < 12 + 16)
        {
            throw new InvalidOperationException("Invalid protected payload.");
        }

        var nonce = payload.AsSpan(0, 12);
        var tag = payload.AsSpan(12, 16);
        var cipher = payload.AsSpan(28);
        var plaintext = new byte[cipher.Length];

        using var aes = new AesGcm(_key);
        aes.Decrypt(nonce, cipher, tag, plaintext);

        return System.Text.Encoding.UTF8.GetString(plaintext);
    }
}
