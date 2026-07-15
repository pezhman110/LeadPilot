using LeadPilot.Domain.Enums;

namespace LeadPilot.Application.Common;

public sealed class DuplicateContactException : Exception
{
    public DuplicateContactException(Guid tenantId, ContactType contactType, string hmacHash, Exception innerException)
        : base("A duplicate contact was detected.", innerException)
    {
        TenantId = tenantId;
        ContactType = contactType;
        HmacHash = hmacHash;
    }

    public Guid TenantId { get; }

    public ContactType ContactType { get; }

    public string HmacHash { get; }
}