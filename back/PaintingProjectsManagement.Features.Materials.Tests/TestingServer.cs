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
using PaintingProjectsManagement.Testing.Core;

namespace PaintingProjectsManagement.Features.Materials.Tests;

public class TestingServer : BaseApplicationTestingServer<Program>
{
    [ClassDataSource<PostgreSqlContainerWrapper>(Shared = SharedType.PerTestSession)]
    public override required PostgreSqlContainerWrapper PostgresContainerWrapper { get; set; } = new();

    [ClassDataSource<RabbitMqContainerWrapper>(Shared = SharedType.PerTestSession)]
    public override required RabbitMqContainerWrapper RabbitContainerWrapper { get; set; } = new();

}