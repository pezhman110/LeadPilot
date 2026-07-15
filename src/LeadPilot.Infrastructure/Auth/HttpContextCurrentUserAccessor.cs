using System.Security.Claims;
using LeadPilot.Application.Abstractions;
using LeadPilot.Application.Common;
using LeadPilot.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace LeadPilot.Infrastructure.Auth;

public sealed class HttpContextCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CurrentUser GetRequiredUser()
    {
        ClaimsPrincipal principal = _httpContextAccessor.HttpContext?.User
            ?? throw new InvalidOperationException("No current HTTP context is available.");

        string? userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        string? roleValue = principal.FindFirstValue(ClaimTypes.Role);
        string? tenantIdValue = principal.FindFirstValue("tenant_id");

        if (!Guid.TryParse(userIdValue, out Guid userId) || !Enum.TryParse<UserRole>(roleValue, ignoreCase: true, out UserRole role))
        {
            throw new InvalidOperationException("The current user principal is missing required claims.");
        }

        Guid? tenantId = Guid.TryParse(tenantIdValue, out Guid parsedTenantId) ? parsedTenantId : null;
        return new CurrentUser(userId, role, tenantId);
    }
}