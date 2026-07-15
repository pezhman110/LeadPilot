using LeadPilot.Domain.Enums;
using LeadPilot.Infrastructure.Configuration;
using LeadPilot.Infrastructure.Normalization;
using LeadPilot.UnitTests.Common;
using Microsoft.Extensions.Options;

namespace LeadPilot.UnitTests;

public sealed class ContactProtectionServiceTests
{
    private readonly ContactProtectionService _service = new(Options.Create(new ContactProtectionOptions
    {
        EncryptionKey = TestKeys.EncryptionKey,
        HashKey = TestKeys.HashKey
    }));

    [Fact]
    public void ProtectProducesTenantScopedDeterministicHashAndRevealDecrypts()
    {
        Guid tenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var protectedOne = _service.Protect(tenantId, ContactType.Email, "alice@example.com", "ALICE@example.com");
        var protectedTwo = _service.Protect(tenantId, ContactType.Email, "alice@example.com", "alice@example.com");
        string revealed = _service.Reveal(protectedOne.Ciphertext, protectedOne.Nonce, protectedOne.AuthenticationTag);

        Assert.Equal(protectedOne.HmacHash, protectedTwo.HmacHash);
        Assert.Equal("a***@example.com", protectedOne.MaskedValue);
        Assert.Equal("ALICE@example.com", revealed);
    }

    [Fact]
    public void ProtectChangesHashAcrossTenants()
    {
        var protectedOne = _service.Protect(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), ContactType.Phone, "+12025550101", "2025550101");
        var protectedTwo = _service.Protect(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), ContactType.Phone, "+12025550101", "2025550101");

        Assert.NotEqual(protectedOne.HmacHash, protectedTwo.HmacHash);
        Assert.Equal("***-***-0101", protectedOne.MaskedValue);
    }
}