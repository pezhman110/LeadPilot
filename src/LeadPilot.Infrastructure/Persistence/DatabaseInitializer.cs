using LeadPilot.Domain.Entities;
using LeadPilot.Domain.Enums;
using LeadPilot.Infrastructure.Auth;
using LeadPilot.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LeadPilot.Infrastructure.Persistence;

public class DatabaseInitializer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeProvider _timeProvider;

    public DatabaseInitializer(IServiceProvider serviceProvider, TimeProvider timeProvider)
    {
        _serviceProvider = serviceProvider;
        _timeProvider = timeProvider;
    }

    public virtual async Task InitializeAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        LeadPilotDbContext dbContext = scope.ServiceProvider.GetRequiredService<LeadPilotDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);

        IOptions<AdminBootstrapOptions> options = scope.ServiceProvider.GetRequiredService<IOptions<AdminBootstrapOptions>>();
        if (string.IsNullOrWhiteSpace(options.Value.Email) || string.IsNullOrWhiteSpace(options.Value.Password))
        {
            return;
        }

        string normalizedEmail = options.Value.Email.Trim().ToLowerInvariant();
        bool exists = await dbContext.AppUsers.AnyAsync(user => user.EmailNormalized == normalizedEmail, cancellationToken).ConfigureAwait(false);
        if (exists)
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