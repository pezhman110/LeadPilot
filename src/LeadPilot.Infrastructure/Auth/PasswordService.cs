using LeadPilot.Application.Abstractions;
using LeadPilot.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace LeadPilot.Infrastructure.Auth;

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<AppUser> _passwordHasher = new();

    public string HashPassword(AppUser user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(AppUser user, string password)
    {
        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}