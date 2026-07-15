namespace LeadPilot.Application.Tenants;

public sealed record TenantDto(Guid Id, string Name, DateTimeOffset CreatedAtUtc);