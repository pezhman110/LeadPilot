using LeadPilot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadPilot.Infrastructure.Persistence.Configurations;

public sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("AppUsers");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.EmailNormalized).HasMaxLength(320).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(1024).IsRequired();
        builder.Property(user => user.Role).HasConversion<int>().IsRequired();
        builder.Property(user => user.CreatedAtUtc).IsRequired();
        builder.HasIndex(user => user.EmailNormalized).IsUnique();
        builder.HasOne(user => user.Tenant)
            .WithMany(tenant => tenant.Users)
            .HasForeignKey(user => user.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}