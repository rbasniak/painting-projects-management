using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace rbkApiModules.Commons.Core;

public class RabbitMqSubscriber : IBrokerSubscriber, IDisposable
{
    private readonly BrokerOptions _options;
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqSubscriber(IOptions<BrokerOptions> options)
    {
        _options = options.Value;
        _factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            DispatchConsumersAsync = true
        };
    }

    public Task SubscribeAsync(string queue, IEnumerable<string> topics, Func<string, byte[], CancellationToken, Task> handler, CancellationToken ct)
    {
        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true);
        _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
        foreach (var topic in topics)
        {
            _channel.QueueBind(queue, _options.Exchange, topic);
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                await handler(ea.RoutingKey, ea.Body.ToArray(), ct);
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch
            {
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };
        _channel.BasicConsume(queue, autoAck: false, consumer);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
