using System.Text;
using System.Text.Json;
using LeadPilot.Application.Abstractions;
using LeadPilot.Application.Contacts;
using LeadPilot.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace LeadPilot.Infrastructure.Messaging;

public sealed class RabbitMqContactImportDispatcher : IContactImportDispatcher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly MessagingOptions _options;

    public RabbitMqContactImportDispatcher(IOptions<MessagingOptions> options)
    {
        _options = options.Value;
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
    }

    public Task PublishAsync(ContactImportMessage message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        IBasicProperties properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        _channel.BasicPublish(exchange: string.Empty, routingKey: _options.QueueName, basicProperties: properties, body: body);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}