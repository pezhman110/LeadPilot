using LeadPilot.Application.Abstractions;
using LeadPilot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeadPilot.Infrastructure.Persistence;

public sealed class LeadPilotDbContext : DbContext, ILeadPilotDbContext
{
    public LeadPilotDbContext(DbContextOptions<LeadPilotDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> AppUsers => Set<AppUser>();

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<ContactPoint> ContactPoints => Set<ContactPoint>();

    public DbSet<ContactRevealAudit> ContactRevealAudits => Set<ContactRevealAudit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LeadPilotDbContext).Assembly);
    }
}