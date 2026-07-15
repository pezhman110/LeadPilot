namespace LeadPilot.Infrastructure.Configuration;

public sealed class PhoneNormalizationOptions
{
    public const string SectionName = "PhoneNormalization";

    public string DefaultRegion { get; set; } = "US";
}