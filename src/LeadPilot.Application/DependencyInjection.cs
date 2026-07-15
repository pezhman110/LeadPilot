using LeadPilot.Application.Auth;
using LeadPilot.Application.Contacts;
using LeadPilot.Application.Managers;
using LeadPilot.Application.Projects;
using LeadPilot.Application.Tenants;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPilot.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<TenantService>();
        services.AddScoped<ProjectService>();
        services.AddScoped<ManagerService>();
        services.AddScoped<ContactImportService>();
        services.AddScoped<ContactPersistenceService>();
        services.AddScoped<ContactQueryService>();
        services.AddScoped<ContactRevealService>();
        return services;
    }
}