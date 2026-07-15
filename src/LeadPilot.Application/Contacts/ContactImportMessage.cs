using LeadPilot.Domain.Enums;

namespace LeadPilot.Application.Contacts;

public sealed record ContactImportMessage(
    Guid TenantId,
    Guid ProjectId,
    ContactType ContactType,
    string HmacHash,
    string MaskedValue,
    byte[] Ciphertext,
    byte[] Nonce,
    byte[] AuthenticationTag,
    DateTimeOffset ImportedAtUtc);