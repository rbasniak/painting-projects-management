using System;
using System.Threading.Tasks;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Npgsql;
using PaintingProjectsManagement.AppHost;

namespace PaintingProjectsManagement.Testing;

public static class TestingAppHost
{
    private static readonly DistributedApplicationFactory _factory = new(typeof(Program));
    private static readonly DistributedApplication _app;

    static TestingAppHost()
    {
        _app = _factory.BuildAsync().GetAwaiter().GetResult();
        _app.StartAsync().GetAwaiter().GetResult();

        PostgresConnectionString = _app.GetConnectionString("ppm-db");
        RabbitMqConnectionString = _app.GetConnectionString("ppm-rabbitmq");

        AppDomain.CurrentDomain.ProcessExit += (_, __) => _app.DisposeAsync().AsTask().Wait();
    }

    public static string PostgresConnectionString { get; }
    public static string RabbitMqConnectionString { get; }

    public static async Task<string> CreateDatabaseAsync(string name)
    {
        var adminBuilder = new NpgsqlConnectionStringBuilder(PostgresConnectionString)
        {
            Database = "postgres"
        };
        await using var conn = new NpgsqlConnection(adminBuilder.ConnectionString);
        await conn.OpenAsync();
        await using (var cmd = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        return new NpgsqlConnectionStringBuilder(PostgresConnectionString)
        {
            Database = name
        }.ConnectionString;
    }

    public static async Task DropDatabaseAsync(string name)
    {
        var adminBuilder = new NpgsqlConnectionStringBuilder(PostgresConnectionString)
        {
            Database = "postgres"
        };
        await using var conn = new NpgsqlConnection(adminBuilder.ConnectionString);
        await conn.OpenAsync();
        await using (var cmd = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{name}\" WITH (FORCE)", conn))
        {
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
