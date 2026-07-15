using LeadPilot.Domain.Enums;

namespace LeadPilot.Domain.Entities;

public sealed class AppUser
{
    public Guid Id { get; set; }

    public string EmailNormalized { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public Guid? TenantId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public Tenant? Tenant { get; set; }

    public ICollection<ContactRevealAudit> ContactRevealAudits { get; } = new List<ContactRevealAudit>();
}