using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.RabbitMq;

namespace PaintingProjectsManagement.Testing.Core;

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