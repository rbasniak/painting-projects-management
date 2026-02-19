using DotNet.Testcontainers.Configurations;
using System.Diagnostics;
using Testcontainers.PostgreSql;

namespace PaintingProjectsManagement.Testing.Core;

// This is needed so the containers can be shared across classes. 
// The wrapped object do not have parameterless contructors, which is required by ClassDataSource.
public class PostgreSqlContainerWrapper 
{
    private static readonly object _lock = new();
    private static PostgreSqlContainer? _sharedContainer;
    private static bool _isInitialized = false;

    private string _instanceId;

    public PostgreSqlContainerWrapper()
    {
        _instanceId = Guid.NewGuid().ToString("N");
        Debug.WriteLine($"*** Wrapper instance created: ::{_instanceId}");
    }

    public PostgreSqlContainer Container
    {
        get
        {
            Debug.WriteLine($"*** Getting container from wrapper: ::{_instanceId}");

            if (!_isInitialized || _sharedContainer == null)
            {
                throw new InvalidOperationException("Container is not initialized.");
            }
            return _sharedContainer;
        }
    }

    public void Initialize()
    {
        lock (_lock)
        {
            Debug.WriteLine($"*** Initializing wrapper: ::{_instanceId} (_isInitialized={_isInitialized})");

            if (!_isInitialized)
            {
                _sharedContainer = new PostgreSqlBuilder()
                    .WithDatabase("postgres")
                    .WithUsername("postgres")
                    .WithPassword("postgrespw")
                    .Build();

                _isInitialized = true;

                Debug.WriteLine($"*** Container created by wrapper: ::{_instanceId}");
            }
            else
            {
                Debug.WriteLine($"*** Container already exists, reusing from wrapper: ::{_instanceId}");
            }
        }
    }

    public async Task StartAsync()
    {
        Debug.WriteLine($"*** Starting container from wrapper: ::{_instanceId}");

        if (!_isInitialized || _sharedContainer == null)
        {
            throw new InvalidOperationException("Container is not initialized.");
        }

        // Only start if not already running
        if (_sharedContainer.State != DotNet.Testcontainers.Containers.TestcontainersStates.Running)
        {
            await _sharedContainer.StartAsync();
            Debug.WriteLine($"*** Container started by wrapper: ::{_instanceId}");
        }
        else
        {
            Debug.WriteLine($"*** Container already running, wrapper: ::{_instanceId}");
        }
    }
}
