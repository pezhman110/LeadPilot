using LeadPilot.Application.Contacts;
using LeadPilot.Domain.Enums;

namespace LeadPilot.Application.Abstractions;

public interface IContactProtectionService
{
    ProtectedContact Protect(Guid tenantId, ContactType contactType, string normalizedValue, string rawValue);

    string Reveal(byte[] ciphertext, byte[] nonce, byte[] authenticationTag);
}