using LeadPilot.Application.Abstractions;
using LeadPilot.Application.Common;
using LeadPilot.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LeadPilot.Application.Contacts;

public sealed class ContactImportService
{
    private readonly IContactImportDispatcher _contactImportDispatcher;
    private readonly IContactNormalizationService _contactNormalizationService;
    private readonly IContactProtectionService _contactProtectionService;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ILeadPilotDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public ContactImportService(
        IContactImportDispatcher contactImportDispatcher,
        IContactNormalizationService contactNormalizationService,
        IContactProtectionService contactProtectionService,
        ICurrentUserAccessor currentUserAccessor,
        ILeadPilotDbContext dbContext,
        TimeProvider timeProvider)
    {
        _contactImportDispatcher = contactImportDispatcher;
        _contactNormalizationService = contactNormalizationService;
        _contactProtectionService = contactProtectionService;
        _currentUserAccessor = currentUserAccessor;
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task<ContactImportSession> BeginAsync(Guid projectId, CancellationToken cancellationToken)
    {
        Common.CurrentUser currentUser = _currentUserAccessor.GetRequiredUser();
        ProjectAccess access = await ResolveProjectAccessAsync(projectId, cancellationToken).ConfigureAwait(false);

        if (currentUser.Role == UserRole.Manager && currentUser.TenantId != access.TenantId)
        {
            throw new ProblemDetailsException(403, "Forbidden", "Managers can only import contacts for their tenant.");
        }

        return new ContactImportSession(access.TenantId, projectId);
    }

    public async Task<int> QueueAsync(ContactImportSession session, ContactImportCandidate candidate, CancellationToken cancellationToken)
    {
        int queuedContacts = 0;

        if (!string.IsNullOrWhiteSpace(candidate.Email))
        {
            await PublishAsync(session, ContactType.Email, candidate.Email, cancellationToken).ConfigureAwait(false);
            queuedContacts++;
        }

        if (!string.IsNullOrWhiteSpace(candidate.Phone))
        {
            await PublishAsync(session, ContactType.Phone, candidate.Phone, cancellationToken).ConfigureAwait(false);
            queuedContacts++;
        }

        return queuedContacts;
    }

    private async Task PublishAsync(ContactImportSession session, ContactType contactType, string rawValue, CancellationToken cancellationToken)
    {
        string normalizedValue = _contactNormalizationService.Normalize(contactType, rawValue);
        ProtectedContact protectedContact = _contactProtectionService.Protect(session.TenantId, contactType, normalizedValue, rawValue);
        ContactImportMessage message = new(
            session.TenantId,
            session.ProjectId,
            contactType,
            protectedContact.HmacHash,
            protectedContact.MaskedValue,
            protectedContact.Ciphertext,
            protectedContact.Nonce,
            protectedContact.AuthenticationTag,
            _timeProvider.GetUtcNow());

        await _contactImportDispatcher.PublishAsync(message, cancellationToken).ConfigureAwait(false);
    }

    private async Task<ProjectAccess> ResolveProjectAccessAsync(Guid projectId, CancellationToken cancellationToken)
    {
        ProjectAccess? access = await _dbContext.Projects
            .Where(project => project.Id == projectId)
            .Select(project => new ProjectAccess(project.Id, project.TenantId))
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (access is null)
        {
            throw new ProblemDetailsException(404, "Project not found", "The specified project does not exist.");
        }

        return access;
    }

    private sealed record ProjectAccess(Guid ProjectId, Guid TenantId);
}