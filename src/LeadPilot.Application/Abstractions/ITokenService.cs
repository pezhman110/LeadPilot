using LeadPilot.Domain.Entities;

namespace LeadPilot.Application.Abstractions;

public interface ITokenService
{
    string CreateToken(AppUser user);
}