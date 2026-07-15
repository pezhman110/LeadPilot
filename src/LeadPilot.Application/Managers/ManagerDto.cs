namespace LeadPilot.Application.Managers;

public sealed record ManagerDto(Guid Id, Guid TenantId, string Email, DateTimeOffset CreatedAtUtc);