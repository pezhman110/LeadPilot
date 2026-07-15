using LeadPilot.Domain.Enums;

namespace LeadPilot.Application.Common;

public sealed record CurrentUser(Guid UserId, UserRole Role, Guid? TenantId);