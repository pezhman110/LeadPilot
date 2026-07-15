using LeadPilot.Domain.Enums;

namespace LeadPilot.Application.Contacts;

public sealed record RevealedContactDto(Guid Id, Guid ProjectId, ContactType ContactType, string Value, DateTimeOffset RevealedAtUtc);