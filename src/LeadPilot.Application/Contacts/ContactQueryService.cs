using LeadPilot.Application.Abstractions;
using LeadPilot.Application.Common;
using LeadPilot.Domain.Entities;
using LeadPilot.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LeadPilot.Application.Contacts;

public sealed class ContactQueryService
{
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ILeadPilotDbContext _dbContext;

    public ContactQueryService(ICurrentUserAccessor currentUserAccessor, ILeadPilotDbContext dbContext)
    {
        _currentUserAccessor = currentUserAccessor;
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ContactListItemDto>> ListAsync(Guid projectId, CancellationToken cancellationToken)
    {
        Project project = await ResolveProjectAsync(projectId, cancellationToken).ConfigureAwait(false);
        EnsureProjectAccess(project.TenantId);

        List<ContactListItemDto> items = await _dbContext.ContactPoints
            .Where(contact => contact.ProjectId == projectId)
            .Select(contact => new ContactListItemDto(
                contact.Id,
                contact.ContactType,
                contact.MaskedValue,
                contact.ImportedAtUtc,
                contact.AccessLevel))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return items
            .OrderBy(contact => contact.ImportedAtUtc)
            .ToList();
    }

    private async Task<Project> ResolveProjectAsync(Guid projectId, CancellationToken cancellationToken)
    {
        Project? project = await _dbContext.Projects
            .SingleOrDefaultAsync(candidate => candidate.Id == projectId, cancellationToken)
            .ConfigureAwait(false);

        if (project is null)
        {
            throw new ProblemDetailsException(404, "Project not found", "The specified project does not exist.");
        }

        return project;
    }

    private void EnsureProjectAccess(Guid tenantId)
    {
        CurrentUser currentUser = _currentUserAccessor.GetRequiredUser();
        if (currentUser.Role == UserRole.Administrator)
        {
            return;
        }

        if (currentUser.Role != UserRole.Manager || currentUser.TenantId != tenantId)
        {
            throw new ProblemDetailsException(403, "Forbidden", "The current user does not have access to this tenant.");
        }
    }
}