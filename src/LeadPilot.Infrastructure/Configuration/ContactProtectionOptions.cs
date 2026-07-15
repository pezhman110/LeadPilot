namespace LeadPilot.Infrastructure.Configuration;

public sealed class ContactProtectionOptions
{
    public const string SectionName = "ContactProtection";

    public string EncryptionKey { get; set; } = string.Empty;

    public string HashKey { get; set; } = string.Empty;
}