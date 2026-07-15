using LeadPilot.Application.Abstractions;
using LeadPilot.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace LeadPilot.Application.Auth;

public sealed class AuthService
{
    private readonly ILeadPilotDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly TimeProvider _timeProvider;

    public AuthService(
        ILeadPilotDbContext dbContext,
        IPasswordService passwordService,
        ITokenService tokenService,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _timeProvider = timeProvider;
    }

    public async Task<AuthTokenResponse> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        string normalizedEmail = email.Trim().ToLowerInvariant();

        Domain.Entities.AppUser? user = await _dbContext.AppUsers
            .SingleOrDefaultAsync(candidate => candidate.EmailNormalized == normalizedEmail, cancellationToken)
            .ConfigureAwait(false);

        if (user is null || !_passwordService.VerifyPassword(user, password))
        {
            throw new ProblemDetailsException(401, "Unauthorized", "Invalid credentials.");
        }

        string token = _tokenService.CreateToken(user);
        DateTimeOffset expiresAtUtc = _timeProvider.GetUtcNow().AddHours(8);
        return new AuthTokenResponse(token, expiresAtUtc);
    }
}