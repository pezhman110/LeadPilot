using LeadPilot.Domain.Enums;

namespace LeadPilot.Domain.Entities;

public sealed class ContactPoint
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ProjectId { get; set; }

    public ContactType ContactType { get; set; }

    public string HmacHash { get; set; } = string.Empty;

    public string MaskedValue { get; set; } = string.Empty;

    public byte[] Ciphertext { get; set; } = Array.Empty<byte>();

    public byte[] Nonce { get; set; } = Array.Empty<byte>();

    public byte[] AuthenticationTag { get; set; } = Array.Empty<byte>();

    public ContactAccessLevel AccessLevel { get; set; }

    public DateTimeOffset ImportedAtUtc { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public Project Project { get; set; } = null!;

    public ICollection<ContactRevealAudit> RevealAudits { get; } = new List<ContactRevealAudit>();
}