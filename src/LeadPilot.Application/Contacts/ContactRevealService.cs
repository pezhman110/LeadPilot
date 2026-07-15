using LeadPilot.Application.Abstractions;
using LeadPilot.Application.Common;
using LeadPilot.Domain.Entities;
using LeadPilot.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LeadPilot.Application.Contacts;

public sealed class ContactRevealService
{
    private readonly IContactProtectionService _contactProtectionService;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ILeadPilotDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public ContactRevealService(
        IContactProtectionService contactProtectionService,
        ICurrentUserAccessor currentUserAccessor,
        ILeadPilotDbContext dbContext,
        TimeProvider timeProvider)
    {
        _contactProtectionService = contactProtectionService;
        _currentUserAccessor = currentUserAccessor;
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task<RevealedContactDto> RevealAsync(Guid projectId, Guid contactId, string? reason, CancellationToken cancellationToken)
    {
        CurrentUser currentUser = _currentUserAccessor.GetRequiredUser();
        if (currentUser.Role != UserRole.Manager || currentUser.TenantId is null)
        {
            throw new ProblemDetailsException(403, "Forbidden", "Only a manager can reveal contact details.");
        }

        ContactPoint? contactPoint = await _dbContext.ContactPoints
            .SingleOrDefaultAsync(candidate => candidate.Id == contactId && candidate.ProjectId == projectId, cancellationToken)
            .ConfigureAwait(false);

        if (contactPoint is null)
        {
            throw new ProblemDetailsException(404, "Contact not found", "The specified contact was not found.");
        }

        if (contactPoint.TenantId != currentUser.TenantId)
        {
            throw new ProblemDetailsException(403, "Forbidden", "Managers can only reveal contacts for their tenant.");
        }

        ContactRevealAudit audit = new()
        {
            Id = Guid.NewGuid(),
            ContactPointId = contactPoint.Id,
            ActorUserId = currentUser.UserId,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            RevealedAtUtc = _timeProvider.GetUtcNow()
        };

        _dbContext.ContactRevealAudits.Add(audit);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        string value = _contactProtectionService.Reveal(contactPoint.Ciphertext, contactPoint.Nonce, contactPoint.AuthenticationTag);
        return new RevealedContactDto(contactPoint.Id, contactPoint.ProjectId, contactPoint.ContactType, value, audit.RevealedAtUtc);
    }
}