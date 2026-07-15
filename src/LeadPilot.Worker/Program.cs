using LeadPilot.Application;
using LeadPilot.Infrastructure;
using LeadPilot.Infrastructure.Persistence;
using LeadPilot.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<ContactImportWorker>();

using IHost host = builder.Build();
using (IServiceScope scope = host.Services.CreateScope())
{
    DatabaseInitializer initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync(CancellationToken.None).ConfigureAwait(false);
}

await host.RunAsync().ConfigureAwait(false);
