using LeadPilot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPilot.Infrastructure.Persistence.Configurations;

public sealed class ContactPointConfiguration : IEntityTypeConfiguration<ContactPoint>
{
    public void Configure(EntityTypeBuilder<ContactPoint> builder)
    {
        builder.ToTable("ContactPoints");
        builder.HasKey(contactPoint => contactPoint.Id);
        builder.Property(contactPoint => contactPoint.ContactType).HasConversion<int>().IsRequired();
        builder.Property(contactPoint => contactPoint.HmacHash).HasMaxLength(128).IsRequired();
        builder.Property(contactPoint => contactPoint.MaskedValue).HasMaxLength(320).IsRequired();
        builder.Property(contactPoint => contactPoint.Ciphertext).IsRequired();
        builder.Property(contactPoint => contactPoint.Nonce).IsRequired();
        builder.Property(contactPoint => contactPoint.AuthenticationTag).IsRequired();
        builder.Property(contactPoint => contactPoint.AccessLevel).HasConversion<int>().IsRequired();
        builder.Property(contactPoint => contactPoint.ImportedAtUtc).IsRequired();
        builder.HasIndex(contactPoint => new { contactPoint.TenantId, contactPoint.ContactType, contactPoint.HmacHash })
            .IsUnique();
        builder.HasOne(contactPoint => contactPoint.Tenant)
            .WithMany(tenant => tenant.ContactPoints)
            .HasForeignKey(contactPoint => contactPoint.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(contactPoint => contactPoint.Project)
            .WithMany(project => project.ContactPoints)
            .HasForeignKey(contactPoint => contactPoint.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}