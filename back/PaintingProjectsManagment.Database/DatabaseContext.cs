using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Features.Materials;
using PaintingProjectsManagement.Features.Models;
using PaintingProjectsManagement.Features.Paints;
using PaintingProjectsManagement.Features.Projects;
using System;

namespace PaintingProjectsManagment.Database;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MaterialConfig).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaintConfig).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ModelConfig).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProjectConfig).Assembly);
    }
}
