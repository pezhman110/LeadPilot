using LeadPilot.Application.Abstractions;
using LeadPilot.Application.Common;
using LeadPilot.Domain.Entities;
using LeadPilot.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LeadPilot.Application.Projects;

public sealed class ProjectService
{
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ILeadPilotDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public ProjectService(ICurrentUserAccessor currentUserAccessor, ILeadPilotDbContext dbContext, TimeProvider timeProvider)
    {
        _currentUserAccessor = currentUserAccessor;
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task<ProjectDto> CreateAsync(Guid tenantId, string name, CancellationToken cancellationToken)
    {
        EnsureAdministrator();

        bool tenantExists = await _dbContext.Tenants
            .AnyAsync(tenant => tenant.Id == tenantId, cancellationToken)
            .ConfigureAwait(false);

        if (!tenantExists)
        {
            throw new ProblemDetailsException(404, "Tenant not found", "The specified tenant does not exist.");
        }

        Project project = new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name.Trim(),
            CreatedAtUtc = _timeProvider.GetUtcNow()
        };

        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new ProjectDto(project.Id, project.TenantId, project.Name, project.CreatedAtUtc);
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