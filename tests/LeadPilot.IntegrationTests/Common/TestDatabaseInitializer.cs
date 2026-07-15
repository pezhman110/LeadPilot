using LeadPilot.Domain.Entities;
using LeadPilot.Domain.Enums;
using LeadPilot.Infrastructure.Auth;
using LeadPilot.Infrastructure.Configuration;
using LeadPilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LeadPilot.IntegrationTests.Common;

internal sealed class TestDatabaseInitializer : DatabaseInitializer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeProvider _timeProvider;

    public TestDatabaseInitializer(IServiceProvider serviceProvider, TimeProvider timeProvider)
        : base(serviceProvider, timeProvider)
    {
        _serviceProvider = serviceProvider;
        _timeProvider = timeProvider;
    }

    public override async Task InitializeAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        LeadPilotDbContext dbContext = scope.ServiceProvider.GetRequiredService<LeadPilotDbContext>();
        await dbContext.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);

        IOptions<AdminBootstrapOptions> options = scope.ServiceProvider.GetRequiredService<IOptions<AdminBootstrapOptions>>();
        if (string.IsNullOrWhiteSpace(options.Value.Email) || string.IsNullOrWhiteSpace(options.Value.Password))
        {
            return;
        }

        string normalizedEmail = options.Value.Email.Trim().ToLowerInvariant();
        if (await dbContext.AppUsers.AnyAsync(user => user.EmailNormalized == normalizedEmail, cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        PasswordService passwordService = scope.ServiceProvider.GetRequiredService<PasswordService>();
        AppUser admin = new()
        {
            Id = Guid.NewGuid(),
            EmailNormalized = normalizedEmail,
            Role = UserRole.Administrator,
            CreatedAtUtc = _timeProvider.GetUtcNow()
        };
        admin.PasswordHash = passwordService.HashPassword(admin, options.Value.Password);
        dbContext.AppUsers.Add(admin);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}