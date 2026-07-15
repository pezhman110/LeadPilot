using LeadPilot.Application.Abstractions;
using LeadPilot.Application.Common;
using LeadPilot.Domain.Entities;
using LeadPilot.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LeadPilot.Application.Contacts;

public sealed class ContactPersistenceService
{
    private readonly ILeadPilotDbContext _dbContext;

    public ContactPersistenceService(ILeadPilotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task PersistAsync(ContactImportMessage message, CancellationToken cancellationToken)
    {
        ContactPoint contactPoint = new()
        {
            Id = Guid.NewGuid(),
            TenantId = message.TenantId,
            ProjectId = message.ProjectId,
            ContactType = message.ContactType,
            HmacHash = message.HmacHash,
            MaskedValue = message.MaskedValue,
            Ciphertext = message.Ciphertext,
            Nonce = message.Nonce,
            AuthenticationTag = message.AuthenticationTag,
            AccessLevel = ContactAccessLevel.ManagerOnly,
            ImportedAtUtc = message.ImportedAtUtc
        };

        _dbContext.ContactPoints.Add(contactPoint);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException exception) when (IsDuplicateContactViolation(exception))
        {
            throw new DuplicateContactException(message.TenantId, message.ContactType, message.HmacHash, exception);
        }
    }

    private static bool IsDuplicateContactViolation(DbUpdateException exception)
    {
        string message = exception.ToString();
        return message.Contains("IX_ContactPoints_TenantId_ContactType_HmacHash", StringComparison.OrdinalIgnoreCase)
            || message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase)
            || message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase);
    }
}