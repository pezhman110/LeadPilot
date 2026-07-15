namespace LeadPilot.Domain.Entities;

public sealed class Project
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public ICollection<ContactPoint> ContactPoints { get; } = new List<ContactPoint>();
}