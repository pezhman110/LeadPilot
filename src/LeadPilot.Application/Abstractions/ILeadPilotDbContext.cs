using LeadPilot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeadPilot.Application.Abstractions;

public interface ILeadPilotDbContext
{
    DbSet<AppUser> AppUsers { get; }

    DbSet<Tenant> Tenants { get; }

    DbSet<Project> Projects { get; }

    DbSet<ContactPoint> ContactPoints { get; }

    DbSet<ContactRevealAudit> ContactRevealAudits { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}