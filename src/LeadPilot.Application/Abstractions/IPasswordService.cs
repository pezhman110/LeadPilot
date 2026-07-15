using LeadPilot.Domain.Entities;

namespace LeadPilot.Application.Abstractions;

public interface IPasswordService
{
    string HashPassword(AppUser user, string password);

    bool VerifyPassword(AppUser user, string password);
}