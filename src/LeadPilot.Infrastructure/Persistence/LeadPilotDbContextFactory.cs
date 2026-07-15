using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LeadPilot.Infrastructure.Persistence;

public sealed class LeadPilotDbContextFactory : IDesignTimeDbContextFactory<LeadPilotDbContext>
{
    public LeadPilotDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<LeadPilotDbContext> builder = new();
        builder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=LeadPilot;Trusted_Connection=True;TrustServerCertificate=True");
        return new LeadPilotDbContext(builder.Options);
    }
}