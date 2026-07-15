using LeadPilot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPilot.Infrastructure.Persistence.Configurations;

public sealed class ContactRevealAuditConfiguration : IEntityTypeConfiguration<ContactRevealAudit>
{
    public void Configure(EntityTypeBuilder<ContactRevealAudit> builder)
    {
        builder.ToTable("ContactRevealAudits");
        builder.HasKey(audit => audit.Id);
        builder.Property(audit => audit.Reason).HasMaxLength(500);
        builder.Property(audit => audit.RevealedAtUtc).IsRequired();
        builder.HasOne(audit => audit.ContactPoint)
            .WithMany(contact => contact.RevealAudits)
            .HasForeignKey(audit => audit.ContactPointId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(audit => audit.ActorUser)
            .WithMany(user => user.ContactRevealAudits)
            .HasForeignKey(audit => audit.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}