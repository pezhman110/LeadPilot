using LeadPilot.Application.Abstractions;
using LeadPilot.Application.Common;
using LeadPilot.Domain.Entities;
using LeadPilot.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LeadPilot.Application.Managers;

public sealed class ManagerService
{
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ILeadPilotDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    private readonly TimeProvider _timeProvider;

    public ManagerService(
        ICurrentUserAccessor currentUserAccessor,
        ILeadPilotDbContext dbContext,
        IPasswordService passwordService,
        TimeProvider timeProvider)
    {
        _currentUserAccessor = currentUserAccessor;
        _dbContext = dbContext;
        _passwordService = passwordService;
        _timeProvider = timeProvider;
    }

    public async Task<ManagerDto> CreateAsync(Guid tenantId, string email, string password, CancellationToken cancellationToken)
    {
        EnsureAdministrator();

        bool tenantExists = await _dbContext.Tenants
            .AnyAsync(tenant => tenant.Id == tenantId, cancellationToken)
            .ConfigureAwait(false);

        if (!tenantExists)
        {
            throw new ProblemDetailsException(404, "Tenant not found", "The specified tenant does not exist.");
        }

        string normalizedEmail = email.Trim().ToLowerInvariant();
        bool emailInUse = await _dbContext.AppUsers
            .AnyAsync(user => user.EmailNormalized == normalizedEmail, cancellationToken)
            .ConfigureAwait(false);

        if (emailInUse)
        {
            throw new ProblemDetailsException(409, "Manager already exists", "The manager email is already in use.");
        }

        AppUser manager = new()
        {
            Id = Guid.NewGuid(),
            EmailNormalized = normalizedEmail,
            Role = UserRole.Manager,
            TenantId = tenantId,
            CreatedAtUtc = _timeProvider.GetUtcNow()
        };
        manager.PasswordHash = _passwordService.HashPassword(manager, password);

        _dbContext.AppUsers.Add(manager);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new ManagerDto(manager.Id, tenantId, manager.EmailNormalized, manager.CreatedAtUtc);
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