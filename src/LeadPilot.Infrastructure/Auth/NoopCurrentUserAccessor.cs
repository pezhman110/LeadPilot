using LeadPilot.Application.Abstractions;
using LeadPilot.Application.Common;

namespace LeadPilot.Infrastructure.Auth;

public sealed class NoopCurrentUserAccessor : ICurrentUserAccessor
{
    public CurrentUser GetRequiredUser()
    {
        throw new InvalidOperationException("No current user is available in this execution context.");
    }
}