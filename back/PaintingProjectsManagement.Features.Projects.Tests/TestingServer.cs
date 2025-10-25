using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using PaintingProjectsManagement.Testing.Core;

namespace PaintingProjectsManagement.Features.Projects.Tests;

// IMPORTANT: if using Podman instead of Docker, make sure Docker Compatibility is enabled in Podman settings.

public class TestingServer : BaseApplicationTestingServer<Program>
{
}