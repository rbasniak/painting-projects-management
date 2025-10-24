using DotNet.Testcontainers.Configurations;
using PaintingProjectsManagement.Testing.Core;

namespace PaintingProjectsManagement.Features.Materials.Tests;

// IMPORTANT: if using Podman instead of Docker, make sure Docker Compatibility is enabled in Podman settings.

public class TestingServer : BaseApplicationTestingServer<Program>
{
    public TestingServer()
    {
        ServerCache2.Cache.Add(InstanceId,  this);
    }
}