namespace LeadPilot.Infrastructure.Configuration;

public sealed class AdminBootstrapOptions
{
    public const string SectionName = "BootstrapAdmin";

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}