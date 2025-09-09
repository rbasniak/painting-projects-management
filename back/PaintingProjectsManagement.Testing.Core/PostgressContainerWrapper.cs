using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;

namespace PaintingProjectsManagement.Testing.Core;

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
