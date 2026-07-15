using LeadPilot.Domain.Enums;

namespace LeadPilot.Application.Abstractions;

public interface IContactNormalizationService
{
    string Normalize(ContactType contactType, string rawValue);
}