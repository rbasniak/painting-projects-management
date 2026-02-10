using Testcontainers.RabbitMq;

namespace PaintingProjectsManagement.Testing.Core;

// This is needed so the containers can be shared across classes. 
// The wrapped object do not have parameterless contructors, which is required by ClassDataSource.
public class RabbitMqContainerWrapper
{
    private static readonly object _lock = new();
    private static RabbitMqContainer? _sharedContainer;
    private static bool _isInitialized = false;

    public RabbitMqContainerWrapper()
    {
    }

    public RabbitMqContainer Container
    {
        get
        {
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
            if (!_isInitialized)
            {
                _sharedContainer = new RabbitMqBuilder()
                    .WithImage("rabbitmq:3.13-management")
                    .WithUsername("guest")
                    .WithPassword("guest")
                    .WithPortBinding(15672, true)
                    .Build();

                _isInitialized = true;
            }
        }
    }

    public async Task StartAsync()
    {
        if (!_isInitialized || _sharedContainer == null)
        {
            throw new InvalidOperationException("Container is not initialized.");
        }

        // Only start if not already running
        if (_sharedContainer.State != DotNet.Testcontainers.Containers.TestcontainersStates.Running)
        {
            await _sharedContainer.StartAsync();
        }
    }
}