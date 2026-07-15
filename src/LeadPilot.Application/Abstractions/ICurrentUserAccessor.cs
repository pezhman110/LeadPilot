using LeadPilot.Application.Common;

namespace LeadPilot.Application.Abstractions;

public interface ICurrentUserAccessor
{
    CurrentUser GetRequiredUser();
}