using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace rbkApiModules.Commons.Core;

public class RabbitMqPublisher : IBrokerPublisher
{
    private readonly BrokerOptions _options;
    private readonly ConnectionFactory _factory;

    public RabbitMqPublisher(IOptions<BrokerOptions> options)
    {
        _options = options.Value;
        _factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };
    }

    public Task PublishAsync(string topic, ReadOnlyMemory<byte> payload, IReadOnlyDictionary<string, object?>? headers, CancellationToken cancellationToken)
    {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true);
        channel.ConfirmSelect();
        var props = channel.CreateBasicProperties();
        props.Persistent = true;

        if (headers is not null && headers.Count > 0)
        {
            props.Headers ??= new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            foreach (var kv in headers)
            {
                props.Headers[kv.Key] = kv.Value;
            }
        }

        channel.BasicPublish(_options.Exchange, topic, props, payload);
        channel.WaitForConfirmsOrDie();

        return Task.CompletedTask;
    }
}
