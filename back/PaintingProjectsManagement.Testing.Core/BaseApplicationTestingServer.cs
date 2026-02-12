using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using rbkApiModules.Commons.Core;
using rbkApiModules.Commons.Core.Messaging;
using rbkApiModules.Commons.Testing;
using Shouldly;
using System.Net.Http.Headers;
using System.Text;

namespace PaintingProjectsManagement.Testing.Core;

public abstract class BaseApplicationTestingServer<TProgram> : RbkTestingServer<TProgram> where TProgram : class
{
    private string _dbName = string.Empty;
    private string _vhost = string.Empty;
    private string _postgresConnectionString = string.Empty;
    private string _rabbitAmqp = string.Empty;

    [ClassDataSource<PostgreSqlContainerWrapper>(Shared = SharedType.PerTestSession)]
    public required virtual PostgreSqlContainerWrapper PostgresContainerWrapper { get; set; } = new();

    [ClassDataSource<RabbitMqContainerWrapper>(Shared = SharedType.PerTestSession)]
    public required virtual RabbitMqContainerWrapper RabbitContainerWrapper { get; set; } = new();

    protected override bool UseHttps => true;

    private static readonly SemaphoreSlim _containerInitializationLock = new(1, 1);

    protected override async Task InitializeApplicationAsync()
    {
        await _containerInitializationLock.WaitAsync();
        try
        {
            // Always initialize wrappers (they check internally if already initialized)
            PostgresContainerWrapper.Initialize();
            RabbitContainerWrapper.Initialize();

            // Always call StartAsync - it's idempotent and ensures the container is tracked
            await Task.WhenAll(
                PostgresContainerWrapper.StartAsync(),
                RabbitContainerWrapper.StartAsync()
            );
        }
        finally
        {
            _containerInitializationLock.Release();
        }

        _dbName = $"db_{InstanceId}";
        _vhost = $"vh_{InstanceId}";

        // Store connection strings
        _postgresConnectionString = PostgresContainerWrapper.Container.GetConnectionString();
        _rabbitAmqp = RabbitContainerWrapper.Container.GetConnectionString();

        await EnsureRabbitVHostAsync(_vhost);
    }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
    }

    protected override IEnumerable<KeyValuePair<string, string>> ConfigureInMemoryOverrides()
    {
        var result = new Dictionary<string, string>();

        _dbName = _dbName ?? $"db_{InstanceId}";
        _vhost = _vhost ?? $"vh_{InstanceId}";

        // Use stored connection strings
        if (!string.IsNullOrEmpty(_postgresConnectionString))
        {
            var csb = new NpgsqlConnectionStringBuilder(_postgresConnectionString)
            {
                Database = _dbName
            };

            result.Add("ConnectionStrings:ppm-database", csb.ToString());
            result.Add("ConnectionStrings:ppm-rabbitmq", _rabbitAmqp);
            result.Add("RabbitMq:VHost:", _rabbitAmqp);
        }

        return result;
    }

    protected override void ConfigureAppConfiguration(WebHostBuilderContext context, IConfigurationBuilder config)
    {

    }

    private async Task EnsureRabbitVHostAsync(string vhost)
    {
        if (RabbitContainerWrapper == null) return;

        var managementBase = new Uri($"http://{RabbitContainerWrapper.Container.Hostname}:{RabbitContainerWrapper.Container.GetMappedPublicPort(15672)}/api/");

        using var http = new HttpClient { BaseAddress = managementBase };
        var token = Convert.ToBase64String(Encoding.ASCII.GetBytes("guest:guest"));
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);

        // Create vhost
        var put = await http.PutAsync($"vhosts/{Uri.EscapeDataString(vhost)}", new StringContent(""));
        put.EnsureSuccessStatusCode();

        // Give permissions to user
        var body = new StringContent("""{"configure":".*","write":".*","read":".*"}""", Encoding.UTF8, "application/json");
        var perm = await http.PutAsync($"permissions/{Uri.EscapeDataString(vhost)}/guest", body);
        perm.EnsureSuccessStatusCode();
    }

    private async Task DropDatabaseIfExists(string dbName)
    {
        if (PostgresContainerWrapper == null) return;

        await using var conn = new NpgsqlConnection(PostgresContainerWrapper.Container.GetConnectionString());
        await conn.OpenAsync();
        await using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{dbName}\" WITH (FORCE);", conn);
        await drop.ExecuteNonQueryAsync();
    }

    private async Task DeleteRabbitVHostAsync(string vhost)
    {
        if (RabbitContainerWrapper == null) return;

        var managementBase = new Uri($"http://{RabbitContainerWrapper.Container.Hostname}:{RabbitContainerWrapper.Container.GetMappedPublicPort(15672)}/api/");

        using var http = new HttpClient { BaseAddress = managementBase };
        var token = Convert.ToBase64String(Encoding.ASCII.GetBytes("guest:guest"));
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
        _ = await http.DeleteAsync($"vhosts/{Uri.EscapeDataString(vhost)}");
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();

        await DropDatabaseIfExists(_dbName);
        await DeleteRabbitVHostAsync(_vhost);
    }

    #region Event Testing Helpers

    /// <summary>
    /// Waits for and asserts that an integration event was published to the outbox
    /// </summary>
    public async Task AssertOutboxMessageAfterAsync<TEvent>(DateTime testStartDate, Func<EventEnvelope<TEvent>, bool> predicate, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(10);

        var delay = 100;
        var steps = timeout.Value.TotalMilliseconds / delay;

        for (int i = 0; i < steps; i++)
        {
            using var context = CreateContext();

            var outboxMessage = await context.Set<IntegrationOutboxMessage>()
                .Where(x => x.Name == GetEventName<TEvent>())
                .Where(x => x.ProcessedUtc > testStartDate)
                .FirstOrDefaultAsync();
                
            if (outboxMessage != null)
            {
                var envelope = JsonEventSerializer.Deserialize(outboxMessage.Payload, typeof(EventEnvelope<TEvent>)) as EventEnvelope<TEvent>;
                if (envelope != null && predicate(envelope))
                {
                    return; // Found matching message
                }
            }
            
            await Task.Delay(delay);
        }
        
        throw new InvalidOperationException($"Expected outbox message not found within {timeout.Value}");
    }

    /// <summary>
    /// Publishes an integration event directly to the outbox for testing
    /// </summary>
    public async Task PublishIntegrationEventAsync<TEvent>(TEvent integrationEvent, string tenantId = "test-tenant", string username = "test-user")
    {
        var envelope = EventEnvelopeFactory.Wrap(integrationEvent, tenantId, username);
        
        using var scope = Services.CreateScope();
        var outbox = scope.ServiceProvider.GetRequiredService<IIntegrationOutbox>();
        await outbox.Enqueue(envelope, CancellationToken.None);
    }

    /// <summary>
    /// Waits for an integration event to be processed (inbox message marked as processed)
    /// </summary>
    public async Task WaitForInboxProcessingAsync<TEvent>(Guid eventId, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(10);
        var endTime = DateTime.UtcNow.Add(timeout.Value);
        
        var delay = 100; // miliseconds
        var steps = timeout.Value.TotalMilliseconds / delay;

        for (int i = 0; i < steps; i++)
        {
            using var context = CreateContext();
            var inboxMessage = await context.Set<InboxMessage>()
                .Where(x => x.EventId == eventId && x.ProcessedUtc != null)
                .FirstOrDefaultAsync();
                
            if (inboxMessage != null)
            {
                return; // Event processed
            }
            
            await Task.Delay(delay);
        }
        
        throw new InvalidOperationException($"Event {eventId} was not processed within {timeout.Value}");
    }

    /// <summary>
    /// Waits for domain events to be processed (domain outbox messages marked as processed)
    /// </summary>
    public async Task WaitForDomainEventProcessingAsync<TEvent>(Guid eventId, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(10);
        var endTime = DateTime.UtcNow.Add(timeout.Value);
        
        var delay = 100; // miliseconds
        var steps = timeout.Value.TotalMilliseconds / delay;

        for (int i = 0; i < steps; i++)
        {
            using var context = CreateContext();
            var outboxMessage = await context.Set<DomainOutboxMessage>()
                .Where(x => x.Name == GetEventName<TEvent>() && x.ProcessedUtc != null)
                .FirstOrDefaultAsync();
                
            if (outboxMessage != null)
            {
                return; // Event processed
            }
            
            await Task.Delay(delay);
        }
        
        throw new InvalidOperationException($"Domain event {eventId} was not processed within {timeout.Value}");
    }

    /// <summary>
    /// Gets the event name for a given event type using the EventName attribute
    /// </summary>
    private static string GetEventName<TEvent>()
    {
        var eventType = typeof(TEvent);
        var eventNameAttribute = eventType.GetCustomAttributes(typeof(EventNameAttribute), false)
            .FirstOrDefault() as EventNameAttribute;
        
        if (eventNameAttribute == null)
        {
            throw new InvalidOperationException($"Event type {eventType.Name} does not have EventName attribute");
        }
        
        return eventNameAttribute.Name;
    }

    /// <summary>
    /// Waits for all pending domain events to be processed
    /// </summary>
    public async Task WaitForAllDomainEventsProcessedAsync(TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(30);
        var endTime = DateTime.UtcNow.Add(timeout.Value);

        var delay = 500;
        var steps = timeout.Value.TotalMilliseconds / delay;

        for (int i = 0; i < steps; i++)
        {
            using var context = CreateContext();
            var pendingCount = await context.Set<DomainOutboxMessage>()
                .Where(x => x.ProcessedUtc == null)
                .CountAsync();
                
            if (pendingCount == 0)
            {
                return; // All events processed
            }
            
            await Task.Delay(delay);
        }
        
        throw new InvalidOperationException($"Not all domain events were processed within {timeout.Value}");
    }

    /// <summary>
    /// Waits for all pending integration events to be processed (published to the brokwe)
    /// </summary>
    public async Task WaitForAllIntegrationEventsPublishedAsync(TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(15);
        var endTime = DateTime.UtcNow.Add(timeout.Value);
        
        var delay = 100;
        var steps = timeout.Value.TotalMilliseconds / delay;

        for (int i = 0; i < steps; i++)
        {
            using var context = CreateContext();
            var pendingCount = await context.Set<IntegrationOutboxMessage>()
                .Where(x => x.ProcessedUtc == null)
                .CountAsync();
                
            if (pendingCount == 0)
            {
                return; // All events processed
            }
            
            await Task.Delay(delay);
        }
        
        throw new InvalidOperationException($"Not all integration events were processed within {timeout.Value}");
    }

    /// <summary>
    /// Waits for all pending integration events to be processed in their consumers
    /// </summary>
    public async Task WaitForAllIntegrationEventsProcessedAsync(DateTime after, Dictionary<Type, int> expectedHandlers, TimeSpan? timeout = null)
    {
        await WaitForAllIntegrationEventsPublishedAsync();

        timeout ??= TimeSpan.FromSeconds(15);

        var delay = 100;
        var steps = timeout.Value.TotalMilliseconds / delay;

        for (int i = 0; i < steps; i++)
        {
            foreach (var kvp in expectedHandlers)
            {
                using var context = CreateContext();

                var processedMessages = await context.Set<InboxMessage>()
                    .Where(x => x.ProcessedUtc != null)
                    .Where(x => x.ProcessedUtc >= after)
                    .Where(x => x.HandlerName == kvp.Key.FullName)
                    .ToListAsync();

                var temp = context.Set<InboxMessage>().Where(x => x.ProcessedUtc != null).OrderByDescending(x => x.ProcessedUtc).ToList();
                var temp2 = context.Set<InboxMessage>().ToList();

                if (processedMessages.Count == kvp.Value)
                {
                    return; // All events processed
                }

                if (processedMessages.Count > kvp.Value)
                {
                    throw new InvalidOperationException($"More messages than expected were processed by consumers");
                }

                await Task.Delay(delay);
            }
        }

        throw new InvalidOperationException($"Not all integration events were processed within {timeout.Value}");
    }


    public void ShouldNotHaveCreatedDomainEvents(DateTime afterDate)
    {
        var context = CreateContext();

        var domainEvents = context.Set<DomainOutboxMessage>().Where(x => x.CreatedUtc >= afterDate).ToArray();
        var integrationEvents = context.Set<IntegrationOutboxMessage>().Where(x => x.CreatedUtc >= afterDate).ToArray();
        var inboxMessages = context.Set<InboxMessage>().Where(x => x.ReceivedUtc >= afterDate).ToArray();

        domainEvents.Length.ShouldBe(0);
    }

    public void ShouldHaveCreatedDomainEvents(DateTime afterDate, Dictionary<Type, int> expectedEvents, out EnvelopeHeader[] events)
    {
        var context = CreateContext();

        var messages = context.Set<DomainOutboxMessage>().Where(x => x.CreatedUtc >= afterDate).ToArray();

        events = messages.Select(x => JsonEventSerializer.DeserializeHeader(x.Payload)).ToArray();

        var uniqueEventTypes = events.GroupBy(x => x.Name).ToArray();

        foreach (var kvp in expectedEvents)
        {
            var searchedEvents = events.Where(x => x.Name == kvp.Key.GetEventName()).ToArray();

            searchedEvents.Length.ShouldBe(kvp.Value, $"Unexpected number of {kvp.Key.GetEventName()} events");
        }
    }

    #endregion
}

public class HumanFriendlyDisplayNameAttribute : DisplayNameFormatterAttribute
{
    protected override string FormatDisplayName(DiscoveredTestContext context)
    {
        try
        {
            int order = 0;

            if (context.TestDetails.AttributesByType.ContainsKey(typeof(NotInParallelAttribute)))
            {
                var attribute = (NotInParallelAttribute)context.TestDetails.AttributesByType[typeof(NotInParallelAttribute)].First();
                order = attribute.Order;
                return $"T{order:000}: {context.TestContext.Metadata.TestDetails.TestName.Replace("_", " ")}";
            }
            else
            {
                return $"{context.TestContext.Metadata.TestDetails.TestName.Replace("_", " ")}";
            }

        }
        catch (Exception ex)
        {
            return context.TestContext.Metadata.TestDetails.TestName + " (FAIL TO COMPUTE NAME)";
        }
    }
}
