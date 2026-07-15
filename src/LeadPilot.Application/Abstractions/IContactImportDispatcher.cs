using LeadPilot.Application.Contacts;

namespace LeadPilot.Application.Abstractions;

public interface IContactImportDispatcher
{
    Task PublishAsync(ContactImportMessage message, CancellationToken cancellationToken);
}