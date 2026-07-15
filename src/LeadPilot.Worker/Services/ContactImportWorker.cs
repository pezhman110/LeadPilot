using System.Text;
using System.Text.Json;
using LeadPilot.Application.Common;
using LeadPilot.Application.Contacts;
using LeadPilot.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace LeadPilot.Worker.Services;

public sealed class ContactImportWorker : BackgroundService
{
    private static readonly Action<ILogger, Guid, Exception?> LogImportFailure = LoggerMessage.Define<Guid>(
        LogLevel.Error,
        new EventId(1, nameof(ContactImportWorker)),
        "Failed to import a contact message for project {ProjectId}");

    private readonly ILogger<ContactImportWorker> _logger;
    private readonly MessagingOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private IConnection? _connection;
    private IModel? _channel;

    public ContactImportWorker(ILogger<ContactImportWorker> logger, IOptions<MessagingOptions> options, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _options = options.Value;
        _serviceProvider = serviceProvider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ConnectionFactory factory = new()
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(_options.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.BasicQos(0, 10, false);

        AsyncEventingBasicConsumer consumer = new(_channel);
        consumer.Received += async (_, eventArgs) =>
        {
            await HandleAsync(eventArgs, stoppingToken).ConfigureAwait(false);
        };

        _channel.BasicConsume(_options.QueueName, autoAck: false, consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }

    private async Task HandleAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
    {
        if (_channel is null)
        {
            return;
        }

        ContactImportMessage? message = JsonSerializer.Deserialize<ContactImportMessage>(Encoding.UTF8.GetString(eventArgs.Body.ToArray()));
        if (message is null)
        {
            _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
            return;
        }

        try
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            ContactPersistenceService persistenceService = scope.ServiceProvider.GetRequiredService<ContactPersistenceService>();
            await persistenceService.PersistAsync(message, cancellationToken).ConfigureAwait(false);
            _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
        }
        catch (DuplicateContactException)
        {
            _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
        }
        catch (Exception exception)
        {
            LogImportFailure(_logger, message.ProjectId, exception);
            _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
        }
    }
}