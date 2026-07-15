using System.Security.Cryptography;
using System.Text;
using LeadPilot.Application.Abstractions;
using LeadPilot.Application.Contacts;
using LeadPilot.Domain.Enums;
using LeadPilot.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace LeadPilot.Infrastructure.Normalization;

public sealed class ContactProtectionService : IContactProtectionService
{
    private readonly byte[] _encryptionKey;
    private readonly byte[] _hashKey;

    public ContactProtectionService(IOptions<ContactProtectionOptions> options)
    {
        _encryptionKey = DecodeKey(options.Value.EncryptionKey, 32, "ContactProtection:EncryptionKey");
        _hashKey = DecodeKey(options.Value.HashKey, 32, "ContactProtection:HashKey");
    }

    public ProtectedContact Protect(Guid tenantId, ContactType contactType, string normalizedValue, string rawValue)
    {
        byte[] plaintext = Encoding.UTF8.GetBytes(rawValue.Trim());
        byte[] nonce = RandomNumberGenerator.GetBytes(AesGcm.NonceByteSizes.MaxSize);
        byte[] ciphertext = new byte[plaintext.Length];
        byte[] tag = new byte[AesGcm.TagByteSizes.MaxSize];

        using (AesGcm aesGcm = new(_encryptionKey, AesGcm.TagByteSizes.MaxSize))
        {
            aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);
        }

        string hashInput = string.Create(
            System.Globalization.CultureInfo.InvariantCulture,
            $"{tenantId:N}:{(int)contactType}:{normalizedValue}");
        byte[] hashBytes = HMACSHA256.HashData(_hashKey, Encoding.UTF8.GetBytes(hashInput));
        string hash = Convert.ToHexString(hashBytes);

        return new ProtectedContact(hash, Mask(contactType, normalizedValue), ciphertext, nonce, tag);
    }

    public string Reveal(byte[] ciphertext, byte[] nonce, byte[] authenticationTag)
    {
        byte[] plaintext = new byte[ciphertext.Length];
        using (AesGcm aesGcm = new(_encryptionKey, AesGcm.TagByteSizes.MaxSize))
        {
            aesGcm.Decrypt(nonce, ciphertext, authenticationTag, plaintext);
        }

        return Encoding.UTF8.GetString(plaintext);
    }

    private static byte[] DecodeKey(string value, int expectedLength, string settingName)
    {
        byte[] key = Convert.FromBase64String(value);
        if (key.Length != expectedLength)
        {
            throw new InvalidOperationException($"{settingName} must decode to {expectedLength} bytes.");
        }

        return key;
    }

    private static string Mask(ContactType contactType, string normalizedValue)
    {
        return contactType switch
        {
            ContactType.Email => MaskEmail(normalizedValue),
            ContactType.Phone => MaskPhone(normalizedValue),
            _ => throw new ArgumentOutOfRangeException(nameof(contactType), contactType, "Unsupported contact type.")
        };
    }

    private static string MaskEmail(string normalizedEmail)
    {
        int atIndex = normalizedEmail.IndexOf('@');
        string localPart = normalizedEmail[..atIndex];
        string domain = normalizedEmail[atIndex..];
        string prefix = localPart.Length <= 1 ? "*" : localPart[..1];
        return prefix + "***" + domain;
    }

    private static string MaskPhone(string normalizedPhone)
    {
        string suffix = normalizedPhone.Length <= 4 ? normalizedPhone : normalizedPhone[^4..];
        return $"***-***-{suffix}";
    }
}