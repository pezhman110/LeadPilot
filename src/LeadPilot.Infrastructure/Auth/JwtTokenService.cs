using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LeadPilot.Application.Abstractions;
using LeadPilot.Domain.Entities;
using LeadPilot.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LeadPilot.Infrastructure.Auth;

public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly TimeProvider _timeProvider;

    public JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider)
    {
        _options = options.Value;
        _timeProvider = timeProvider;
    }

    public string CreateToken(AppUser user)
    {
        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_options.SigningKey));
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);
        DateTimeOffset issuedAt = _timeProvider.GetUtcNow();
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.EmailNormalized),
            new(ClaimTypes.Role, user.Role.ToString())
        ];

        if (user.TenantId is Guid tenantId)
        {
            claims.Add(new Claim("tenant_id", tenantId.ToString()));
        }

        JwtSecurityToken token = new(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: issuedAt.UtcDateTime,
            expires: issuedAt.AddHours(8).UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}