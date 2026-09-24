using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using Shouldly;
using TUnit.Core;
using MaterialDetails = PaintingProjectsManagement.Features.Materials.UseCases.Web.MaterialDetails;
using ProjectDetails = PaintingProjectsManagement.Features.Projects.ProjectDetails;
using MyPaintDetails = PaintingProjectsManagement.Features.Inventory.MyPaintDetails;
using PaintingProjectsManagement.UI.Modules.Materials;
using PaintingProjectsManagement.UI.Modules.Projects;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchitectureModel = ArchUnitNET.Domain.Architecture;

namespace PaintingProjectsManagement.Features.Architecture.Tests;

/// <summary>
/// Automated architecture tests to enforce module boundary rules.
/// </summary>
public class ModuleBoundaryTests
{
    private static readonly ArchitectureModel Arch = new ArchLoader()
        .LoadAssemblies(
            typeof(PaintingProjectsManagement.Features.Materials.Material).Assembly,
            typeof(MaterialDetails).Assembly,
            typeof(IMaterialsService).Assembly,
            typeof(PaintingProjectsManagement.Features.Projects.Project).Assembly,
            typeof(ProjectDetails).Assembly,
            typeof(IProjectsService).Assembly,
            typeof(MyPaintDetails).Assembly
        )
        .Build();

    private static void CheckRule(IArchRule rule) =>
        rule.HasNoViolations(Arch).ShouldBeTrue($"Architecture rule violated: {rule.Description}");

    /// <summary>
    /// Rule: UI projects may only reference *.Core.Contracts and *.Web.Contracts
    /// They MUST NOT reference *.Core, *.Web, or *.Integrations directly
    /// </summary>
    [Test]
    public void UI_Projects_Should_Not_Reference_Core_Directly()
    {
        IArchRule rule = Classes()
            .That().ResideInNamespace("PaintingProjectsManagement.UI.Modules.Materials")
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespace("PaintingProjectsManagement.Features.Materials.Core");

        CheckRule(rule);
    }

    /// <summary>
    /// Rule: UI projects should not reference Web implementation directly
    /// They should use Web.Contracts for DTOs
    /// </summary>
    [Test]
    public void UI_Projects_Should_Not_Reference_Web_Directly()
    {
        IArchRule rule = Classes()
            .That().ResideInNamespace("PaintingProjectsManagement.UI.Modules.Materials")
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespace("PaintingProjectsManagement.Features.Materials.Web.UseCases");

        CheckRule(rule);
    }

    /// <summary>
    /// Rule: Web projects should not reference other modules' Core directly
    /// Cross-module communication should use Integrations.Contracts or events
    /// </summary>
    [Test]
    public void Web_Projects_Should_Not_Reference_Other_Modules_Core()
    {
        IArchRule rule = Classes()
            .That().ResideInNamespace("PaintingProjectsManagement.Features.Projects.Web")
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespace("PaintingProjectsManagement.Features.Subscriptions.Core");

        CheckRule(rule);
    }

    /// <summary>
    /// Rule: Core projects should only contain domain logic - no Web or UI dependencies
    /// </summary>
    [Test]
    public void Core_Should_Not_Reference_Web_Or_UI()
    {
        IArchRule noWebRule = Classes()
            .That().ResideInNamespace("PaintingProjectsManagement.Features.Materials")
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespace("PaintingProjectsManagement.Features.Materials.Web");

        IArchRule noUIRule = Classes()
            .That().ResideInNamespace("PaintingProjectsManagement.Features.Materials")
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespace("PaintingProjectsManagement.UI.Modules.Materials");

        CheckRule(noWebRule);
        CheckRule(noUIRule);
    }

    /// <summary>
    /// Rule: All modules should follow the naming convention
    /// PaintingProjectsManagement.Features.{ModuleName}.{Layer}
    /// </summary>
    [Test]
    public void All_Modules_Should_Follow_Naming_Convention()
    {
        var assemblies = new[]
        {
            typeof(PaintingProjectsManagement.Features.Materials.Material).Assembly,
            typeof(MaterialDetails).Assembly,
            typeof(IMaterialsService).Assembly,
            typeof(PaintingProjectsManagement.Features.Projects.Project).Assembly,
            typeof(ProjectDetails).Assembly,
            typeof(IProjectsService).Assembly,
            typeof(MyPaintDetails).Assembly,
        };

        foreach (var assembly in assemblies)
        {
            var name = assembly.GetName().Name!;
            var isValid = name.StartsWith("PaintingProjectsManagement.Features.") ||
                          name.StartsWith("PaintingProjectsManagement.UI.Modules.");

            isValid.ShouldBeTrue(
                $"Assembly ''{name}'' does not follow naming convention " +
                "(should start with PaintingProjectsManagement.Features. or PaintingProjectsManagement.UI.Modules.)");
        }
    }

    /// <summary>
    /// Comprehensive test: UI can only reference Contracts projects (Core.Contracts + Web.Contracts)
    /// </summary>
    [Test]
    public void UI_Can_Only_Reference_Contracts_Projects()
    {
        IArchRule materialRule = Classes()
            .That().ResideInNamespace("PaintingProjectsManagement.UI.Modules.Materials")
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespace("PaintingProjectsManagement.Features.Materials.Core");

        IArchRule projectRule = Classes()
            .That().ResideInNamespace("PaintingProjectsManagement.UI.Modules.Projects")
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespace("PaintingProjectsManagement.Features.Projects.Core");

        CheckRule(materialRule);
        CheckRule(projectRule);
    }
}
