using LeadPilot.Application.Abstractions;
using LeadPilot.Infrastructure.Auth;
using LeadPilot.Infrastructure.Configuration;
using LeadPilot.Infrastructure.Messaging;
using LeadPilot.Infrastructure.Normalization;
using LeadPilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LeadPilot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<ContactProtectionOptions>(configuration.GetSection(ContactProtectionOptions.SectionName));
        services.Configure<MessagingOptions>(configuration.GetSection(MessagingOptions.SectionName));
        services.Configure<PhoneNormalizationOptions>(configuration.GetSection(PhoneNormalizationOptions.SectionName));
        services.Configure<AdminBootstrapOptions>(configuration.GetSection(AdminBootstrapOptions.SectionName));

        services.AddDbContext<LeadPilotDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("SqlServer"));
        });
        services.AddScoped<ILeadPilotDbContext>(provider => provider.GetRequiredService<LeadPilotDbContext>());
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddScoped<PasswordService>();
        services.AddScoped<IPasswordService>(provider => provider.GetRequiredService<PasswordService>());
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IContactNormalizationService, ContactNormalizationService>();
        services.AddScoped<IContactProtectionService, ContactProtectionService>();
        services.AddScoped<ICurrentUserAccessor, NoopCurrentUserAccessor>();
        services.AddSingleton<IContactImportDispatcher, RabbitMqContactImportDispatcher>();
        services.AddScoped<DatabaseInitializer>();

        IHealthChecksBuilder healthChecks = services.AddHealthChecks();
        string sqlServerConnectionString = configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("ConnectionStrings:SqlServer must be configured.");

        healthChecks.AddSqlServer(sqlServerConnectionString, name: "sqlserver", failureStatus: HealthStatus.Unhealthy);

        string? redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            healthChecks.AddRedis(redisConnectionString, name: "redis", failureStatus: HealthStatus.Unhealthy);
        }

        MessagingOptions rabbitOptions = configuration.GetSection(MessagingOptions.SectionName).Get<MessagingOptions>() ?? new MessagingOptions();
        healthChecks.AddRabbitMQ(
            rabbitConnectionString: $"amqp://{rabbitOptions.UserName}:{rabbitOptions.Password}@{rabbitOptions.HostName}:{rabbitOptions.Port}",
            name: "rabbitmq",
            failureStatus: HealthStatus.Unhealthy);

        return services;
    }
}