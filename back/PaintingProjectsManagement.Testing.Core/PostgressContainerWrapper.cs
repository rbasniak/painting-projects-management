using DotNet.Testcontainers.Configurations;
using System.Diagnostics;
using Testcontainers.PostgreSql;

namespace PaintingProjectsManagement.Testing.Core;

// This is needed so the containers can be shared across classes. 
// The wrapped object do not have parameterless contructors, which is required by ClassDataSource.
public class PostgreSqlContainerWrapper 
{
    private string _instanceId;
    private bool _isInitialized = false;
    private PostgreSqlContainer? _container;

    public PostgreSqlContainerWrapper()
    {
        _instanceId = Guid.NewGuid().ToString("N");

        Debug.WriteLine($"*** Instance created: ::{_instanceId}");
    }

    public PostgreSqlContainer Container
    {
        get
        {
            Debug.WriteLine($"*** Getting: ::{_instanceId} ");

            if (!_isInitialized)
            {
                throw new InvalidOperationException("Container is not initialized.");
            }
            return _container ?? throw new InvalidOperationException("Container is initialized but is null.");
        }
    }

    public void Initialize()
    {
        Debug.WriteLine($"*** Initializing: ::{_instanceId} (_isInitialized={_isInitialized})");
        if (!_isInitialized)
        {
            _container = new PostgreSqlBuilder()
                .WithDatabase("postgres")
                .WithUsername("postgres")
                .WithPassword("postgrespw")
                .WithReuse(true) // Either this or random ports in between assemblies
                .Build();

            _isInitialized = true;

            Debug.WriteLine($"*** Initialized: ::{_instanceId} (_isInitialized={_isInitialized})");
        }
    }

    public async Task StartAsync()
    {
        Debug.WriteLine($"*** Starting: ::{_instanceId}");
        if (_isInitialized && _container != null)
        {
            Debug.WriteLine($"*** Started: ::{_instanceId}");
            await _container.StartAsync();
        }
        else
        {
            throw new InvalidOperationException("Container is not initialized or is not healthy.");
        }
    }
}
