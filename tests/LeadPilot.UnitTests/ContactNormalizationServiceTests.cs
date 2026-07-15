using LeadPilot.Domain.Enums;
using LeadPilot.Infrastructure.Configuration;
using LeadPilot.Infrastructure.Normalization;
using Microsoft.Extensions.Options;

namespace LeadPilot.UnitTests;

public sealed class ContactNormalizationServiceTests
{
    private readonly ContactNormalizationService _service = new(Options.Create(new PhoneNormalizationOptions { DefaultRegion = "US" }));

    [Fact]
    public void NormalizePhoneReturnsE164()
    {
        string normalized = _service.Normalize(ContactType.Phone, "(202) 555-0101");

        Assert.Equal("+12025550101", normalized);
    }

    [Fact]
    public void NormalizeEmailTrimsAndLowercases()
    {
        string normalized = _service.Normalize(ContactType.Email, "  ALICE@Example.COM ");

        Assert.Equal("alice@example.com", normalized);
    }
}