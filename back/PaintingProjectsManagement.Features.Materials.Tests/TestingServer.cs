using PaintingProjectsManagement.Testing.Core;

namespace PaintingProjectsManagement.Features.Materials.Tests;

public class TestingServer : BaseApplicationTestingServer<Program>
{
    [ClassDataSource<PostgreSqlContainerWrapper>(Shared = SharedType.PerTestSession)]
    public override required PostgreSqlContainerWrapper PostgresContainerWrapper { get; set; } = new();

    [ClassDataSource<RabbitMqContainerWrapper>(Shared = SharedType.PerTestSession)]
    public override required RabbitMqContainerWrapper RabbitContainerWrapper { get; set; } = new();

}