namespace LeadPilot.Infrastructure.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "LeadPilot";

    public string Audience { get; set; } = "LeadPilot";

    public string SigningKey { get; set; } = string.Empty;
}