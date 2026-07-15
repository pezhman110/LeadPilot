using LeadPilot.Application.Abstractions;
using LeadPilot.Application.Common;
using LeadPilot.Domain.Enums;
using LeadPilot.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using PhoneNumbers;

namespace LeadPilot.Infrastructure.Normalization;

public sealed class ContactNormalizationService : IContactNormalizationService
{
    private readonly PhoneNormalizationOptions _options;
    private readonly PhoneNumberUtil _phoneNumberUtil = PhoneNumberUtil.GetInstance();

    public ContactNormalizationService(IOptions<PhoneNormalizationOptions> options)
    {
        _options = options.Value;
    }

    public string Normalize(ContactType contactType, string rawValue)
    {
        return contactType switch
        {
            ContactType.Email => NormalizeEmail(rawValue),
            ContactType.Phone => NormalizePhone(rawValue),
            _ => throw new ArgumentOutOfRangeException(nameof(contactType), contactType, "Unsupported contact type.")
        };
    }

    private static string NormalizeEmail(string rawValue)
    {
        string normalized = rawValue.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized) || !normalized.Contains('@'))
        {
            throw new ProblemDetailsException(400, "Invalid email", "The CSV file contains an invalid email address.");
        }

        return normalized;
    }

    private string NormalizePhone(string rawValue)
    {
        try
        {
            PhoneNumber phoneNumber = _phoneNumberUtil.Parse(rawValue, _options.DefaultRegion);
            if (!_phoneNumberUtil.IsValidNumber(phoneNumber))
            {
                throw new ProblemDetailsException(400, "Invalid phone number", "The CSV file contains an invalid phone number.");
            }

            return _phoneNumberUtil.Format(phoneNumber, PhoneNumberFormat.E164);
        }
        catch (NumberParseException)
        {
            throw new ProblemDetailsException(400, "Invalid phone number", "The CSV file contains an invalid phone number.");
        }
    }
}