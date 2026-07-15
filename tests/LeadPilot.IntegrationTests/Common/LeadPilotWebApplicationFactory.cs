using System.Data.Common;
using LeadPilot.Application.Abstractions;
using LeadPilot.Infrastructure.Persistence;
using LeadPilot.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeadPilot.IntegrationTests.Common;

public sealed class LeadPilotWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:SqlServer", "Server=(localdb)\\mssqllocaldb;Database=LeadPilotTests;Trusted_Connection=True;TrustServerCertificate=True");
        builder.UseSetting("ConnectionStrings:Redis", string.Empty);
        builder.UseSetting("Jwt:Issuer", "LeadPilot.Tests");
        builder.UseSetting("Jwt:Audience", "LeadPilot.Tests");
        builder.UseSetting("Jwt:SigningKey", TestKeys.JwtSigningKey);
        builder.UseSetting("ContactProtection:EncryptionKey", TestKeys.EncryptionKey);
        builder.UseSetting("ContactProtection:HashKey", TestKeys.HashKey);
        builder.UseSetting("RabbitMq:HostName", "localhost");
        builder.UseSetting("RabbitMq:Port", "5672");
        builder.UseSetting("RabbitMq:UserName", "guest");
        builder.UseSetting("RabbitMq:Password", "guest");
        builder.UseSetting("BootstrapAdmin:Email", "admin@leadpilot.test");
        builder.UseSetting("BootstrapAdmin:Password", "AdminPassword123!");
        builder.UseSetting("PhoneNormalization:DefaultRegion", "US");

        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            Dictionary<string, string?> settings = new()
            {
                ["ConnectionStrings:SqlServer"] = "Server=(localdb)\\mssqllocaldb;Database=LeadPilotTests;Trusted_Connection=True;TrustServerCertificate=True",
                ["ConnectionStrings:Redis"] = string.Empty,
                ["Jwt:Issuer"] = "LeadPilot.Tests",
                ["Jwt:Audience"] = "LeadPilot.Tests",
                ["Jwt:SigningKey"] = TestKeys.JwtSigningKey,
                ["ContactProtection:EncryptionKey"] = TestKeys.EncryptionKey,
                ["ContactProtection:HashKey"] = TestKeys.HashKey,
                ["RabbitMq:HostName"] = "localhost",
                ["RabbitMq:Port"] = "5672",
                ["RabbitMq:UserName"] = "guest",
                ["RabbitMq:Password"] = "guest",
                ["BootstrapAdmin:Email"] = "admin@leadpilot.test",
                ["BootstrapAdmin:Password"] = "AdminPassword123!",
                ["PhoneNormalization:DefaultRegion"] = "US"
            };
            configurationBuilder.AddInMemoryCollection(settings);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<LeadPilotDbContext>>();
            services.RemoveAll<LeadPilotDbContext>();
            services.RemoveAll<ILeadPilotDbContext>();
            services.RemoveAll<IContactImportDispatcher>();
            services.RemoveAll<DatabaseInitializer>();

            _connection ??= new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            services.AddDbContext<LeadPilotDbContext>(options => options.UseSqlite(_connection));
            services.AddScoped<ILeadPilotDbContext>(provider => provider.GetRequiredService<LeadPilotDbContext>());
            services.AddSingleton<IContactImportDispatcher, ImmediateContactImportDispatcher>();
            services.AddScoped<DatabaseInitializer, TestDatabaseInitializer>();
        });
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
        }
    }

    public async Task<int> CountRevealAuditsAsync()
    {
        using IServiceScope scope = Services.CreateScope();
        LeadPilotDbContext dbContext = scope.ServiceProvider.GetRequiredService<LeadPilotDbContext>();
        return await dbContext.ContactRevealAudits.CountAsync().ConfigureAwait(false);
    }
}