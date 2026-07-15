namespace LeadPilot.Domain.Entities;

public sealed class ContactRevealAudit
{
    public Guid Id { get; set; }

    public Guid ContactPointId { get; set; }

    public Guid ActorUserId { get; set; }

    public string? Reason { get; set; }

    public DateTimeOffset RevealedAtUtc { get; set; }

    public ContactPoint ContactPoint { get; set; } = null!;

    public AppUser ActorUser { get; set; } = null!;
}