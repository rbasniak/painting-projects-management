using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Net.Http.Headers;
using System.Text;
using DotNet.Testcontainers.Builders;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace PaintingProjectsManagement.Features.Materials.Tests;

public class TestingServer : RbkTestingServer<Program>
{
    private string _dbName = string.Empty;
    private string _vhost = string.Empty;
    private string _postgresConnectionString = string.Empty;
    private string _rabbitAmqp = string.Empty;

    [ClassDataSource<PostgreSqlContainerWrapper>(Shared = SharedType.PerTestSession)]
    public required PostgreSqlContainerWrapper PostgresContainerWrapper { get; set; } = new();

    [ClassDataSource<RabbitMqContainerWrapper>(Shared = SharedType.PerTestSession)]
    public required RabbitMqContainerWrapper RabbitContainerWrapper { get; set; } = new();

    protected override bool UseHttps => true;

    private static bool AreContainersInitialized = false;

    protected override async Task InitializeApplicationAsync()
    {
        if (!AreContainersInitialized)
        {
            PostgresContainerWrapper.Initialize();
            RabbitContainerWrapper.Initialize();

            await Task.WhenAll(
                PostgresContainerWrapper.StartAsync(),
                RabbitContainerWrapper.StartAsync()
            );

            AreContainersInitialized = true;
        }

        _dbName = $"db_{InstanceId}";
        _vhost = $"vh_{InstanceId}";

        TestEvents.Add($"Using Postgres container: {PostgresContainerWrapper.Container.Id} with database {_dbName}");

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
        await DropDatabaseIfExists(_dbName);
        await DeleteRabbitVHostAsync(_vhost);
    }
}

// This is needed so the containers can be shared across classes. 
// The wrapped object do not have parameterless contructors, which is required by ClassDataSource.
public class PostgreSqlContainerWrapper
{
    private bool _isInitialized = false;
    private PostgreSqlContainer? _container;

    public PostgreSqlContainerWrapper()
    {
    }

    public PostgreSqlContainer Container
    {
        get
        {
            if (!_isInitialized)
            {
                 throw new InvalidOperationException("Container is not initialized.");
            }
            return _container ?? throw new InvalidOperationException("Container is initialized but is null.");
        }
    }

    public void Initialize()
    {
        if (!_isInitialized)
        {
            _container = new PostgreSqlBuilder()
                .WithDatabase("postgres")
                .WithUsername("postgres")
                .WithPassword("postgrespw")
                .Build();

            _isInitialized = true;
        }
    }

    public async Task StartAsync()
    {
        if (_isInitialized && _container != null)
        {
            await _container.StartAsync();
        }
        else
        {
            throw new InvalidOperationException("Container is not initialized or is not healthy.");
        }
    }
}

// This is needed so the containers can be shared across classes. 
// The wrapped object do not have parameterless contructors, which is required by ClassDataSource.
public class RabbitMqContainerWrapper
{
    private bool _isInitialized = false;
    private RabbitMqContainer? _container;

    public RabbitMqContainerWrapper()
    {
    }

    public RabbitMqContainer Container
    {
        get
        {
            if (!_isInitialized)
            {
               throw new InvalidOperationException("Container is not initialized.");
            }
            return _container ?? throw new InvalidOperationException("Container is initialized but is null.");
        }
    }

    public void Initialize()
    {
        if (!_isInitialized)
        {
            _container = new RabbitMqBuilder()
                .WithImage("rabbitmq:3.13-management")
                .WithPortBinding(15672, 15672)
                .WithUsername("guest")
                .WithPassword("guest")
                .Build();

            _isInitialized = true;
        }
    }

    public async Task StartAsync()
    {
        if (_isInitialized && _container != null)
        {
            await _container.StartAsync();
        }
        else
        {
            throw new InvalidOperationException("Container is not initialized or is not healthy.");
        }
    }
}