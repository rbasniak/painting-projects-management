using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using rbkApiModules.Commons.Testing;
using System.Net.Http.Headers;
using System.Text;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace PaintingProjectsManagement.Testing.Core;

public abstract class BaseApplicationTestingServer<TProgram> : RbkTestingServer<TProgram> where TProgram : class
{
    private string _dbName = string.Empty;
    private string _vhost = string.Empty;
    private string _postgresConnectionString = string.Empty;
    private string _rabbitAmqp = string.Empty;

    public required virtual PostgreSqlContainerWrapper PostgresContainerWrapper { get; set; } = new();
    public required virtual RabbitMqContainerWrapper RabbitContainerWrapper { get; set; } = new();

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