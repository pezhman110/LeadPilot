using LeadPilot.Domain.Enums;

namespace LeadPilot.Application.Contacts;

public sealed record ContactListItemDto(
    Guid Id,
    ContactType ContactType,
    string MaskedValue,
    DateTimeOffset ImportedAtUtc,
    ContactAccessLevel AccessLevel);