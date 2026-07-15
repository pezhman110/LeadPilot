using LeadPilot.Application.Abstractions;
using LeadPilot.Application.Common;
using LeadPilot.Domain.Entities;
using LeadPilot.Domain.Enums;

namespace LeadPilot.Application.Tenants;

public sealed class TenantService
{
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ILeadPilotDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public TenantService(ICurrentUserAccessor currentUserAccessor, ILeadPilotDbContext dbContext, TimeProvider timeProvider)
    {
        _currentUserAccessor = currentUserAccessor;
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task<TenantDto> CreateAsync(string name, CancellationToken cancellationToken)
    {
        EnsureAdministrator();

        Tenant tenant = new()
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            CreatedAtUtc = _timeProvider.GetUtcNow()
        };

        _dbContext.Tenants.Add(tenant);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new TenantDto(tenant.Id, tenant.Name, tenant.CreatedAtUtc);
    }

    private void EnsureAdministrator()
    {
        CurrentUser currentUser = _currentUserAccessor.GetRequiredUser();
        if (currentUser.Role != UserRole.Administrator)
        {
            throw new ProblemDetailsException(403, "Forbidden", "Administrator access is required.");
        }
    }
}