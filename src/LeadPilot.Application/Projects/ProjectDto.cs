namespace LeadPilot.Application.Projects;

public sealed record ProjectDto(Guid Id, Guid TenantId, string Name, DateTimeOffset CreatedAtUtc);