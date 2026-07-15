using LeadPilot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPilot.Infrastructure.Persistence.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");
        builder.HasKey(project => project.Id);
        builder.Property(project => project.Name).HasMaxLength(200).IsRequired();
        builder.Property(project => project.CreatedAtUtc).IsRequired();
        builder.HasIndex(project => new { project.TenantId, project.Name }).IsUnique();
        builder.HasOne(project => project.Tenant)
            .WithMany(tenant => tenant.Projects)
            .HasForeignKey(project => project.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}