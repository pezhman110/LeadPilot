using LeadPilot.Application.Abstractions;
using LeadPilot.Application.Common;
using LeadPilot.Application.Contacts;
using Microsoft.Extensions.DependencyInjection;

namespace LeadPilot.IntegrationTests.Common;

internal sealed class ImmediateContactImportDispatcher : IContactImportDispatcher
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ImmediateContactImportDispatcher(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task PublishAsync(ContactImportMessage message, CancellationToken cancellationToken)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ContactPersistenceService service = scope.ServiceProvider.GetRequiredService<ContactPersistenceService>();

        try
        {
            await service.PersistAsync(message, cancellationToken).ConfigureAwait(false);
        }
        catch (DuplicateContactException)
        {
        }
    }
}