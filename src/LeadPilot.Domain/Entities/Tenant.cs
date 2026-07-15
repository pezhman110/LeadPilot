namespace LeadPilot.Domain.Entities;

public sealed class Tenant
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public ICollection<Project> Projects { get; } = new List<Project>();

    public ICollection<AppUser> Users { get; } = new List<AppUser>();

    public ICollection<ContactPoint> ContactPoints { get; } = new List<ContactPoint>();
}