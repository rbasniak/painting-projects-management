using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Features.Projects;
using rbkApiModules.Commons.Relational;
using System.Diagnostics;

namespace PaintingProjectsManagment.Database;

public class ProjectsSeed : IDeferredSeedStep
{
    public string Id => "2026-03-18 13h04m: Seed initial projects data";

    public EnvironmentUsage EnvironmentUsage => EnvironmentUsage.Production | EnvironmentUsage.Development | EnvironmentUsage.Staging | EnvironmentUsage.Testing;

    public Type DbContextType => typeof(DatabaseContext);

    public async Task ExecuteAsync(DbContext context, IServiceProvider serviceProvider)
    {
        async Task ConsumeMaterial(Project project, string materialName, double quantity, MaterialUnit unit)
        {
            Material? material = null;

            var timeout = DateTime.UtcNow.AddSeconds(15);

            do
            {
                material = context.Set<Material>().FirstOrDefault(x => x.Name == materialName);

                if (material == null)
                {
                    // This seed depends on events from the Materials module to be processed before proceeding
                    // This delays is meant to solve this when database is first created and seeded
                    await Task.Delay(500);
                }
            } while (material == null && DateTime.UtcNow < timeout);

            if (material == null)
            {
                Debugger.Break();

                throw new InvalidOperationException($"Material '{materialName}' not found in the database.");
            }

            project.ConsumeMaterial(material.Id, quantity, unit);
        }

        #region Archangel

        var archangelProject = new Project("rodrigo.basniak", "Archangel", new DateTime(2025, 07, 15), modelId: null);

        ConsumeMaterial(archangelProject, DatabaseSeed.MaterialMagnet10x5, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(archangelProject, DatabaseSeed.MaterialMagnet5x3, 3, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(archangelProject, DatabaseSeed.MaterialMagnet3x2, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(archangelProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 165 + 120 + 75 + 25 + 95 + 60, MaterialUnit.Gram).GetAwaiter().GetResult();
        ConsumeMaterial(archangelProject, DatabaseSeed.MaterialVallejoPrimer, 200, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(archangelProject, DatabaseSeed.MaterialGswPrimer, 200, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(archangelProject, DatabaseSeed.MaterialGswChrome, 375 + 50, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(archangelProject, DatabaseSeed.MaterialArmyPainterAcrylic, 50 + 25 + 10 + 10 + 20 + 10 + 10 + 15 + 5 + 10 + 5 + 5 + 25 + 25 + 50, MaterialUnit.Drop).GetAwaiter().GetResult();

        archangelProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (218.0 + 135.0 + 110.0) / 20.0);

        archangelProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 4);
        archangelProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 1.5);
        archangelProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 1.5);
        archangelProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 0.5 + 2 + 13);
        archangelProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 28), 15);
        archangelProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 08, 10), 8);

        context.Add(archangelProject);

        context.SaveChanges();

        #endregion

        #region Illidan

        var illidanProject = new Project("rodrigo.basniak", "Illidan Stormrage", new DateTime(2025, 08, 15), modelId: null);

        ConsumeMaterial(illidanProject, DatabaseSeed.MaterialMagnet10x5, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(illidanProject, DatabaseSeed.MaterialMagnet8x4, 18, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(illidanProject, DatabaseSeed.MaterialMagnet3x2, 8, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(illidanProject, DatabaseSeed.MaterialPaintMixingCupCorrugated, 10, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(illidanProject, DatabaseSeed.MaterialMaskingTape3m25mm, 3, MaterialUnit.Meter).GetAwaiter().GetResult();
        ConsumeMaterial(illidanProject, DatabaseSeed.MaterialVallejoLiquidMask, 2, MaterialUnit.Mililiter).GetAwaiter().GetResult();
        ConsumeMaterial(illidanProject, DatabaseSeed.MaterialDisposableBrush, 2, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(illidanProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, (210 + 160 + 205), MaterialUnit.Gram).GetAwaiter().GetResult();

        ConsumeMaterial(illidanProject, DatabaseSeed.MaterialVallejoPrimer, 150, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(illidanProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 68 + 19 + 30, MaterialUnit.Spray).GetAwaiter().GetResult();
        ConsumeMaterial(illidanProject, DatabaseSeed.MaterialGswVarnish, 50, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(illidanProject, DatabaseSeed.MaterialArmyPainterAcrylic, 345 + 50 + 25 + 115 + 50 + 75, MaterialUnit.Drop).GetAwaiter().GetResult();

        illidanProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (260.0 + 206.0 + 204.0) / 20.0);

        illidanProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 6);
        illidanProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 1.5);
        illidanProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 2);
        illidanProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 0.5 + 6 + 3 + 1 + 3 + 3 + 1.5 + 3);

        context.Add(illidanProject);

        context.SaveChanges();

        #endregion

        #region Sylvanas

        var sylvanasProject = new Project("rodrigo.basniak", "Sylvanas Windrunner", new DateTime(2025, 08, 15), modelId: null);

        ConsumeMaterial(sylvanasProject, DatabaseSeed.MaterialMagnet10x5, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(sylvanasProject, DatabaseSeed.MaterialMagnet8x4, 16, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(sylvanasProject, DatabaseSeed.MaterialMagnet6x4, 10, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(sylvanasProject, DatabaseSeed.MaterialMagnet3x2, 8, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(sylvanasProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, (200 + 235 + 150), MaterialUnit.Gram).GetAwaiter().GetResult();

        ConsumeMaterial(sylvanasProject, DatabaseSeed.MaterialVallejoPrimer, 225, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(sylvanasProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 75, MaterialUnit.Spray).GetAwaiter().GetResult();
        ConsumeMaterial(sylvanasProject, DatabaseSeed.MaterialGswAcrylic, 125 + 50 + 5 + 15, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(sylvanasProject, DatabaseSeed.MaterialGswChrome, 100, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(sylvanasProject, DatabaseSeed.MaterialTamiyaTS80, 15, MaterialUnit.Spray).GetAwaiter().GetResult();
        ConsumeMaterial(sylvanasProject, DatabaseSeed.MaterialArmyPainterAcrylic, 10 + 20 + 50 + 10 + 15 + 25 + 50 + 50 + 10, MaterialUnit.Drop).GetAwaiter().GetResult();

        sylvanasProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (223.0 + 200.0 + 115.0) / 20.0);

        sylvanasProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 6);
        sylvanasProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 1.5);
        sylvanasProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 2);
        sylvanasProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 0.5 + 1.5 + 5 + 6 + 6 + 1.5 + 1.5);

        context.Add(sylvanasProject);

        context.SaveChanges();

        #endregion

        #region Lara Croft

        var laraProject = new Project("rodrigo.basniak", "Lara Croft", new DateTime(2025, 08, 15), modelId: null);

        ConsumeMaterial(laraProject, DatabaseSeed.MaterialMagnet10x5, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(laraProject, DatabaseSeed.MaterialMagnet8x4, 14, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(laraProject, DatabaseSeed.MaterialMagnet6x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(laraProject, DatabaseSeed.MaterialTamiyaTS80, 70 + 5 + 15 + 50, MaterialUnit.Spray).GetAwaiter().GetResult();

        ConsumeMaterial(laraProject, DatabaseSeed.MaterialMaskingTapeTamiya10mm, 30, MaterialUnit.Centimeter).GetAwaiter().GetResult();

        laraProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (105.0 + 177.0 + 186.0) / 20.0);
        ConsumeMaterial(laraProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, (180.0 + 165.5 + 195.0), MaterialUnit.Gram).GetAwaiter().GetResult();

        ConsumeMaterial(laraProject, DatabaseSeed.MaterialVallejoPrimer, 175, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(laraProject, DatabaseSeed.MaterialArmyPainterAcrylic, 25 + 95 + 150 + 30 + 75 + 50 + 40 + 155, MaterialUnit.Drop).GetAwaiter().GetResult();

        laraProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 6);
        laraProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 1);
        laraProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 3);
        laraProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 0.5 + 1 + 0.5 + 3 + 1 + 15 + 2 + 2);

        context.Add(laraProject);

        context.SaveChanges();

        #endregion

        #region Camus of Aquarius

        var camusProject = new Project("rodrigo.basniak", "Camus of Aquarius", new DateTime(2025, 08, 15), modelId: null);

        camusProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (190.0 + 210.0 + 185.0 + 145.0 + 135.0) / 20.0);
        ConsumeMaterial(camusProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, (175 + 125 + 335 + 60 + 150 + 45 + 160 + 50 + 130 + 75), MaterialUnit.Gram).GetAwaiter().GetResult();

        ConsumeMaterial(camusProject, DatabaseSeed.MaterialMagnet10x5, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(camusProject, DatabaseSeed.MaterialMagnet8x4, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(camusProject, DatabaseSeed.MaterialMagnet6x4, 10, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(camusProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 92 + 65 + 35, MaterialUnit.Spray).GetAwaiter().GetResult();

        ConsumeMaterial(camusProject, DatabaseSeed.MaterialVallejoPrimer, 100 + 100 + 100, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(camusProject, DatabaseSeed.MaterialArmyPainterAcrylic, 100 + 75 + 15 + 25 + 25 + 125 + 50 + 50 + 25, MaterialUnit.Drop).GetAwaiter().GetResult();

        camusProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 4);
        camusProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 1.5);
        camusProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 4.5);
        camusProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 1 + 3 + 2 + 2 + 1.5 + 2 + 4);

        context.Add(camusProject);

        context.SaveChanges();

        #endregion

        #region Kratos

        var kratosProject = new Project("rodrigo.basniak", "Kratos", new DateTime(2025, 08, 15), modelId: null);

        ConsumeMaterial(kratosProject, DatabaseSeed.MaterialMagnet10x5, 34, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, DatabaseSeed.MaterialMagnet8x4, 9, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, DatabaseSeed.MaterialMagnet6x4, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, DatabaseSeed.MaterialDspiaeSandingDisk, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, DatabaseSeed.MaterialModellingPutty, 5, MaterialUnit.Gram).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, DatabaseSeed.MaterialMaskingTapeGeneric5mm, 1, MaterialUnit.Meter).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, DatabaseSeed.MaterialDisposableBrush, 5, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, DatabaseSeed.MaterialPaintMixingCupCorrugated, 10, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, DatabaseSeed.MaterialPaintMixingCupNonCorrugated, 5, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, DatabaseSeed.MaterialMaskingTapeGeneric10mm, 200, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, DatabaseSeed.MaterialMaskingTape3m25mm, 100, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, DatabaseSeed.MaterialArmyPainterSnow, 150 * 0.75, MaterialUnit.Gram).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, DatabaseSeed.MaterialMaskingTapeTamiya20mm, 100, MaterialUnit.Centimeter).GetAwaiter().GetResult();

        kratosProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (160 + 160 + 155 + 155 + 135 + 125 + 190) / 20.0);
        ConsumeMaterial(kratosProject, DatabaseSeed.MaterialSunluAbsResin, 195 + 75 + 225 + 65 + 195 + 70 + 135 + 55 + 165 + 60 + 155 + 60 + 150 + 80, MaterialUnit.Gram).GetAwaiter().GetResult();

        ConsumeMaterial(kratosProject, DatabaseSeed.MaterialVallejoPrimer, 175 + 375 + 50 + 15, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, DatabaseSeed.MaterialVallejoVarnish, 500 + 25 + 25, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, DatabaseSeed.MaterialGswVarnish, 25, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 110 + 40 + 105, MaterialUnit.Spray).GetAwaiter().GetResult();

        ConsumeMaterial(kratosProject, DatabaseSeed.MaterialArmyPainterAcrylic, 245 + 640, MaterialUnit.Drop).GetAwaiter().GetResult();

        kratosProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 9.5);
        kratosProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 2);
        kratosProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 9);
        kratosProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 1.75 + 4.5 + 2 + 4 + 10 + 10 + 8 + 7.5 + 5);

        context.Add(kratosProject);

        context.SaveChanges();

        #endregion

        #region Starlight

        var starligtProject = new Project("rodrigo.basniak", "Starlight", new DateTime(2025, 08, 15), modelId: null);

        ConsumeMaterial(starligtProject, DatabaseSeed.MaterialMagnet10x5, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(starligtProject, DatabaseSeed.MaterialMagnet8x4, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(starligtProject, DatabaseSeed.MaterialMagnet6x4, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(starligtProject, DatabaseSeed.MaterialMagnet3x2, 4, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(starligtProject, DatabaseSeed.MaterialTamiyaTS80, 25 + 25 + 25 + 20, MaterialUnit.Spray).GetAwaiter().GetResult();

        ConsumeMaterial(starligtProject, DatabaseSeed.MaterialDisposableBrush, 2, MaterialUnit.Unit).GetAwaiter().GetResult();

        starligtProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (115.0 + 85.0) / 20.0);
        ConsumeMaterial(starligtProject, DatabaseSeed.MaterialJayoAbsResin, 150 + 90 + 40, MaterialUnit.Gram).GetAwaiter().GetResult();

        ConsumeMaterial(starligtProject, DatabaseSeed.MaterialVallejoPrimer, 50, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(starligtProject, DatabaseSeed.MaterialArmyPainterAcrylic, 50 + 50 + 50 + 20 + 25 + 40 + 5 + 20 + 145 + 75 + 10 + 25 + 10, MaterialUnit.Drop).GetAwaiter().GetResult();

        starligtProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 2.5);
        starligtProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 1.5);
        starligtProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 1);
        starligtProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 0.5 + 1 + 3 + 1.5 + 2 + 2);

        context.Add(starligtProject);

        context.SaveChanges();

        #endregion

        #region Harley Quinn 

        var harleyProject = new Project("rodrigo.basniak", "Harley Quinn", new DateTime(2025, 08, 15), modelId: null);

        ConsumeMaterial(harleyProject, DatabaseSeed.MaterialMagnet10x5, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(harleyProject, DatabaseSeed.MaterialMagnet8x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(harleyProject, DatabaseSeed.MaterialMagnet6x4, 6, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(harleyProject, DatabaseSeed.MaterialVallejoPrimer, 150, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(harleyProject, DatabaseSeed.MaterialArmyPainterAcrylic, 40 + 75 + 40 + 425 + 45, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(harleyProject, DatabaseSeed.MaterialTamiyaTS80, 125 + 30 + 35 + 45 + 45, MaterialUnit.Spray).GetAwaiter().GetResult();
        ConsumeMaterial(harleyProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 25, MaterialUnit.Spray).GetAwaiter().GetResult();

        ConsumeMaterial(harleyProject, DatabaseSeed.MaterialDisposableBrush, 2, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(harleyProject, DatabaseSeed.MaterialMaskingTape3m25mm, 60, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(harleyProject, DatabaseSeed.MaterialMaskingTapeGeneric25mm, 40, MaterialUnit.Centimeter).GetAwaiter().GetResult();

        harleyProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (152 + 105) / 20.0);
        ConsumeMaterial(harleyProject, DatabaseSeed.MaterialSunluAbsResin, 235 + 115, MaterialUnit.Gram).GetAwaiter().GetResult();


        harleyProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 3);
        harleyProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 0.5);
        harleyProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 2);
        harleyProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 1 + 2.5 + 8 + 9 + 2.5 + 6 + 2.5 + 1 + 1.5);

        context.Add(harleyProject);

        context.SaveChanges();

        #endregion

        #region Sadie Adler

        var sadieProject = new Project("rodrigo.basniak", "Sadie Adler", new DateTime(2025, 08, 15), modelId: null);

        ConsumeMaterial(sadieProject, DatabaseSeed.MaterialMagnet10x5, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(sadieProject, DatabaseSeed.MaterialMagnet8x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(sadieProject, DatabaseSeed.MaterialMagnet6x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(sadieProject, DatabaseSeed.MaterialMagnet5x3, 4, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(sadieProject, DatabaseSeed.MaterialVallejoPrimer, 150, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(sadieProject, DatabaseSeed.MaterialArmyPainterAcrylic, 10 + 30 + 90 + 5 + 3 + 15 + 20 + 20 + 5 + 15 + 5 + 15, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(sadieProject, DatabaseSeed.MaterialVallejoAcrylic, 45, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(sadieProject, DatabaseSeed.MaterialAtomAcrylic, 10 + 7 + 25 + 2 + 5 + 20 + 30, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(sadieProject, DatabaseSeed.MaterialDisposableBrush, 1, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(sadieProject, DatabaseSeed.MaterialDspiaeSandingDisk, 1, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(sadieProject, DatabaseSeed.MaterialMaskingTapeTamiya10mm, 20, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(sadieProject, DatabaseSeed.MaterialMaskingTapeGeneric25mm, 20, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(sadieProject, DatabaseSeed.MaterialMaskingTapeGeneric10mm, 20 + 15, MaterialUnit.Centimeter).GetAwaiter().GetResult();

        sadieProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (70 + 150) / 20.0);
        ConsumeMaterial(sadieProject, DatabaseSeed.MaterialJayoAbsResin, (75 + 35 + 100 + 45), MaterialUnit.Gram).GetAwaiter().GetResult();

        sadieProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 3);
        sadieProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 1);
        sadieProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 1.5);
        sadieProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 0.5 + 0.5 + 13 + 2);

        context.Add(sadieProject);

        context.SaveChanges();

        #endregion

        #region Hulkbuster and Iron Man

        var hulkbusterProject = new Project("rodrigo.basniak", "Hulkbuster and Iron Man", new DateTime(2025, 08, 15), modelId: null);

        hulkbusterProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (156 + 143 + 145 + 153 + 138 + 135 + 165 + 115 + 132 + 148 + 131 + 150 + 90 + 136 + 96 + 98) / 17.5);
        ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialJayoAbsResin, (185 + 175 + 220 + 160 + 175 + 165 + 200 + 185 + 180 + 185 + 215 + 210 + 170 + 235 + 95 + 105), MaterialUnit.Gram).GetAwaiter().GetResult();

        ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialMagnet10x5, 10, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialMagnet8x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialDspiaeSandingDisk, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialDisposableBrush, 10, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialPaintMixingCupCorrugated, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialGlooves, 25, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialNailWoodSwab, 50, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialGswPrimer, 1040, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialVallejoPrimer, 215, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialAkChrome, 115, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialGswChrome, 50 + 100 + 100 + 100 + 39 + 100 + 100 + 100 + 75, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialVallejoAcrylic, 10, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialTamiyaClear, 75 + 50 + 40 + 200 + 6 + 20 + 250 + 250, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialAmmoCandy, 75 + 10, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialArmyPainterAcrylic, 45 + 50 + 40 + 10 + 30 + 50 + 20 + 10 + 10 + 50 + 75 + 10 + 20, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialGswVarnish, 200 + 100 + 50 + 75, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialAmmoTransparator, 20, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialAmmoDrybrush, 50, MaterialUnit.Drop).GetAwaiter().GetResult();


        hulkbusterProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 12), 4);
        hulkbusterProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 12), 2 + 2 + 3.5 + 1.5 + 12 + 4);
        hulkbusterProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 12), 6);
        hulkbusterProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 12), 7.5);
        hulkbusterProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 12), 3 + 1 + 6 + 4.5 + 2 + 5 + 12 + 2 + 1.5 + 1 + 1.5 + 3 + 4);

        context.Add(hulkbusterProject);

        context.SaveChanges();

        #endregion

        #region Harry Potter

        var harryProject = new Project("rodrigo.basniak", "Harry Potter", new DateTime(2025, 08, 15), modelId: null);

        harryProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (140 + 190 + 58 + 140) / 20);
        ConsumeMaterial(harryProject, DatabaseSeed.MaterialJayoAbsResin, 115 + 30 + 185 + 55 + 51 + 25 + 190 + 85, MaterialUnit.Gram).GetAwaiter().GetResult();

        harryProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 12), 1);
        harryProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 12), 5);
        harryProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 12), 2);
        harryProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 12), 1.5);

        ConsumeMaterial(harryProject, DatabaseSeed.MaterialMagnet10x5, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, DatabaseSeed.MaterialMagnet8x4, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, DatabaseSeed.MaterialMaskingTapeTamiya10mm, 10 + 15, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, DatabaseSeed.MaterialMaskingTapeGeneric25mm, 15 + 25, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, DatabaseSeed.MaterialMaskingTapeGeneric10mm, 100, MaterialUnit.Centimeter).GetAwaiter().GetResult();

        ConsumeMaterial(harryProject, DatabaseSeed.MaterialVallejoPrimer, 300, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, DatabaseSeed.MaterialGswChrome, 30, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, DatabaseSeed.MaterialArmyPainterAcrylic, 100 + 25 + 50 + 5 + 25 + 20 + 25 + 5 + 5 + 5 + 5 + 5 + 5 + 5 + 5 + 10, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, DatabaseSeed.MaterialVallejoAcrylic, 5 + 15 + 10 + 5 + 25 + 10 + 15, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 50, MaterialUnit.Spray).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, DatabaseSeed.MaterialAtomAcrylic, 30 + 10 + 5 + 4 + 7, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, DatabaseSeed.MaterialAmmoAcrylic, 5 + 40 + 10 + 10, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, DatabaseSeed.MaterialVallejoVarnish, 30 + 55, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, DatabaseSeed.MaterialAmmoDrybrush, 5, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, DatabaseSeed.MaterialAmmoTransparator, 130 + 40 + 10 + 15, MaterialUnit.Drop).GetAwaiter().GetResult();

        harryProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 12), 0.5 + 5 + 1 + 1.5 + 6.5 + 5 + 1);

        context.Add(harryProject);

        context.SaveChanges();

        #endregion

        #region Raven

        var ravenProject = new Project("rodrigo.basniak", "Raven", new DateTime(2025, 11, 01), modelId: null);

        ravenProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (190 + 210) / 20);
        ConsumeMaterial(ravenProject, DatabaseSeed.MaterialJayoAbsResin, (170 + 55 + 195 + 50), MaterialUnit.Gram).GetAwaiter().GetResult();

        ravenProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        ravenProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 5);
        ravenProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 1);
        ravenProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 2);

        ConsumeMaterial(ravenProject, DatabaseSeed.MaterialMagnet10x5, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, DatabaseSeed.MaterialMagnet8x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, DatabaseSeed.MaterialMagnet4x3, 2, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, DatabaseSeed.MaterialMagnet3x2, 1, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, DatabaseSeed.MaterialMagnet6x4, 2, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(ravenProject, DatabaseSeed.MaterialMaskingTape3m25mm, 15, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, DatabaseSeed.MaterialMaskingTapeGeneric25mm, 10, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, DatabaseSeed.MaterialMaskingTapeTamiya10mm, 30, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, DatabaseSeed.MaterialDisposableBrush, 2, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, DatabaseSeed.MaterialNailWoodSwab, 35, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(ravenProject, DatabaseSeed.MaterialVallejoPrimer, 250, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, DatabaseSeed.MaterialArmyPainterAcrylic, 25 + 10 + 5 + 50 + 30 + 20 + 15 + 15 + 20, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, DatabaseSeed.MaterialVallejoAcrylic, 5 + 10 + 10, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, DatabaseSeed.MaterialAmmoDrybrush, 5, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, DatabaseSeed.MaterialAtomAcrylic, 18, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, DatabaseSeed.MaterialAmmoTransparator, 30, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, DatabaseSeed.MaterialTamiyaTS80, 55, MaterialUnit.Spray).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 25, MaterialUnit.Spray).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, DatabaseSeed.MaterialVallejoVarnish, 15, MaterialUnit.Drop).GetAwaiter().GetResult();

        ravenProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 3 + 4 + 6 + 0.5);

        context.Add(ravenProject);

        context.SaveChanges();

        #endregion

        #region Lace

        var laceProject = new Project("rodrigo.basniak", "Lady Lace", new DateTime(2025, 11, 01), modelId: null);

        laceProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (200 + 30) / 20);
        ConsumeMaterial(laceProject, DatabaseSeed.MaterialJayoAbsResin, (200 + 40), MaterialUnit.Gram).GetAwaiter().GetResult();

        laceProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        laceProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 1.5);
        laceProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 0.5);
        laceProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 0);

        ConsumeMaterial(laceProject, DatabaseSeed.MaterialMagnet10x5, 8, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(laceProject, DatabaseSeed.MaterialVallejoPrimer, 250, MaterialUnit.Drop).GetAwaiter().GetResult();

        //ConsumeMaterial(laceProject, DatabaseSeed.MaterialArmyPainterAcrylic, 245 + 640, MaterialUnit.Drop).GetAwaiter().GetResult();
        //ConsumeMaterial(laceProject, DatabaseSeed.MaterialArmyPainterAcrylic, 245 + 640, MaterialUnit.Drop).GetAwaiter().GetResult();
        //ConsumeMaterial(laceProject, DatabaseSeed.MaterialArmyPainterAcrylic, 245 + 640, MaterialUnit.Drop).GetAwaiter().GetResult();
        //ConsumeMaterial(laceProject, DatabaseSeed.MaterialArmyPainterAcrylic, 245 + 640, MaterialUnit.Drop).GetAwaiter().GetResult();

        // laceProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 0.0);

        context.Add(laceProject);

        context.SaveChanges();

        #endregion

        #region Aphrodite

        var aphroditeProject = new Project("rodrigo.basniak", "Aphrodite of Pisces", new DateTime(2025, 12, 10), modelId: null);

        aphroditeProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (90 + 210 + 215 + 210 + 120 + 205 + 200 + 155 + 140) / 17);
        ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialJayoAbsResin, 110 + 50 + 160 + 70 + 220 + 105 + 160 + 105, MaterialUnit.Gram).GetAwaiter().GetResult();
        ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialAnycubricClearResin, 70 + 70 + 90 + 50 + 180 + 85 + 115 + 45 + 85 + 55, MaterialUnit.Gram).GetAwaiter().GetResult();

        aphroditeProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 2);
        aphroditeProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 18);
        aphroditeProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 4.5);
        aphroditeProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 3);

        ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialMagnet10x5, 12, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialMagnet8x4, 7, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialMagnet6x4, 6, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialVallejoPrimer, 125 + 30 + 35 + 30 + 15, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialGswPrimer, 350, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialTamiyaClear, 180 + 60 + 20 + 50 + 10, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialAmmoCandy, 40 + 80 + 10 + 30, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialArmyPainterAcrylic, 20 + 60 + 10 + 25 + 40 + 10, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialGswVarnish, 50, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialAtomAcrylic, 30, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialGswChrome, 220, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialAmmoDrybrush, 50, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialGswAcrylic, 30, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialBiltemaGlossVarnish, 30 + 70 + 30 + 67 + 35 + 55 + 40 + 55, MaterialUnit.Spray).GetAwaiter().GetResult();
        ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialTamiyaTS80, 40, MaterialUnit.Spray).GetAwaiter().GetResult();

        ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialPaintMixingCupCorrugated, 3, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialNailWoodSwab, 40, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialDisposableBrush, 10, MaterialUnit.Unit).GetAwaiter().GetResult();

        aphroditeProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 12, 12), 1.5 + 0.5 + 1 + 1 + 3 + 2 + 2 + 1.5 + 1 + 3 + 1 + 2 + 1);

        context.Add(aphroditeProject);

        context.SaveChanges();

        #endregion

        #region Iron Man and Tony Stark

        var tonyStarkProject = new Project("rodrigo.basniak", "Iton Man and Tony Stark", new DateTime(2025, 01, 01), modelId: null);

        tonyStarkProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (200 + 125 + 90) / 13);
        ConsumeMaterial(tonyStarkProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 325 + 115 + 170 + 65 + 80 + 40, MaterialUnit.Gram).GetAwaiter().GetResult();

        tonyStarkProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        tonyStarkProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 6);
        tonyStarkProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 1);
        tonyStarkProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 2);

        ConsumeMaterial(tonyStarkProject, DatabaseSeed.MaterialMagnet10x5, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(tonyStarkProject, DatabaseSeed.MaterialMagnet8x4, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(tonyStarkProject, DatabaseSeed.MaterialMagnet6x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(tonyStarkProject, DatabaseSeed.MaterialVallejoPrimer, 75 + 135, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(tonyStarkProject, DatabaseSeed.MaterialArmyPainterAcrylic, 35 + 20 + 10 + 5 + 15 + 10 + 5 + 10 + 60 + 5 + 10 + 5 + 30 + 30 + 10 + 10, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(tonyStarkProject, DatabaseSeed.MaterialTamiyaClear, 20, MaterialUnit.Mililiter).GetAwaiter().GetResult();

        tonyStarkProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 1.5 + 1 + 1.5 + 0.5 + 2 + 1.5 + 1.5 + 3 + 2 + 3.5 + 2 + 3 + 2 + 2);

        context.Add(tonyStarkProject);

        context.SaveChanges();

        #endregion

        #region Kassandra

        var kassandraProject = new Project("rodrigo.basniak", "Kassandra", new DateTime(2025, 01, 01), modelId: null);

        kassandraProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (155 + 105 + 215 + 175) / 15);
        ConsumeMaterial(kassandraProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 245 + 60 + 70 + 35 + 155 + 100 + 250, MaterialUnit.Gram).GetAwaiter().GetResult();

        kassandraProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        kassandraProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 6);
        kassandraProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 1);
        kassandraProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 2);

        ConsumeMaterial(kassandraProject, DatabaseSeed.MaterialMagnet10x5, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(kassandraProject, DatabaseSeed.MaterialMagnet8x4, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(kassandraProject, DatabaseSeed.MaterialMagnet6x4, 2, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(kassandraProject, DatabaseSeed.MaterialMagnet3x2, 12, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(kassandraProject, DatabaseSeed.MaterialVallejoPrimer, 23, MaterialUnit.Mililiter).GetAwaiter().GetResult();

        ConsumeMaterial(kassandraProject, DatabaseSeed.MaterialArmyPainterAcrylic, 15 + 40 + 10 + 10 + 5 + 70 + 40 + 2 + 30 + 5 + 30 + 15 + 10 + 310, MaterialUnit.Drop).GetAwaiter().GetResult();

        kassandraProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 1.5 + 1 + 1.5 + 1.5 + 0.25 + 1.25 + 2 + 1.5 + 1 + 3 + 7 + 3 + 5 + 2);

        context.Add(kassandraProject);

        context.SaveChanges();

        #endregion

        #region Rogue vs Miss Marvel

        var missMarvelProject = new Project("rodrigo.basniak", "Rogue vs Miss Marvel", new DateTime(2025, 01, 01), modelId: null);

        missMarvelProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (160 + 90 + 200) / 15.0);
        ConsumeMaterial(missMarvelProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 155 + 70 + 100 + 50 + 135 + 75, MaterialUnit.Gram).GetAwaiter().GetResult();

        missMarvelProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        missMarvelProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 6);
        missMarvelProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 1);
        missMarvelProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 3);

        ConsumeMaterial(missMarvelProject, DatabaseSeed.MaterialMagnet10x5, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(missMarvelProject, DatabaseSeed.MaterialMagnet8x4, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(missMarvelProject, DatabaseSeed.MaterialMagnet6x4, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(missMarvelProject, DatabaseSeed.MaterialMagnet3x2, 4, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(missMarvelProject, DatabaseSeed.MaterialVallejoPrimer, 225, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(missMarvelProject, DatabaseSeed.MaterialArmyPainterAcrylic, 60 + 10 + 15 + 15 + 30 + 20 + 10 + 10 + 5 + 5 + 5 + 45 + 20 + 20 + 45 + 20 + 20 + 20 + 5 + 50 + 100 + 35 + 10 + 40 + 10 + 40 + 20 + 20, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(missMarvelProject, DatabaseSeed.MaterialVallejoVarnish, 150, MaterialUnit.Drop).GetAwaiter().GetResult();

        missMarvelProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 1.5 + 0.25 + 0.25 + 2 + 1.5 + 2 + 4.5 + 5 + 12);

        context.Add(missMarvelProject);

        context.SaveChanges();

        #endregion

        #region Yennefer

        var yenneferProject = new Project("rodrigo.basniak", "Yennefer", new DateTime(2025, 01, 01), modelId: null);

        yenneferProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (85 + 140) / 15.0);
        ConsumeMaterial(yenneferProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 95 + 20 + 210 + 35, MaterialUnit.Gram).GetAwaiter().GetResult();

        yenneferProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        yenneferProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 4);
        yenneferProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 0.5);
        yenneferProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 1);

        ConsumeMaterial(yenneferProject, DatabaseSeed.MaterialMagnet6x4, 12, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(yenneferProject, DatabaseSeed.MaterialVallejoPrimer, 75, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(yenneferProject, DatabaseSeed.MaterialArmyPainterAcrylic, 10 + 10 + 20 + 20 + 15 + 15 + 10 + 10 + 5 + 5 + 5 + 5 + 30 + 10 + 50 + 5 + 5 + 10 + 10 + 5 + 10, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(yenneferProject, DatabaseSeed.MaterialVallejoVarnish, 75, MaterialUnit.Drop).GetAwaiter().GetResult();

        yenneferProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 25);

        context.Add(yenneferProject);

        context.SaveChanges();

        #endregion

        #region Wonder Woman

        var wonderWomanProject = new Project("rodrigo.basniak", "Wonder Woman", new DateTime(2025, 01, 01), modelId: null);

        wonderWomanProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (145 + 155) / 15.0);
        ConsumeMaterial(wonderWomanProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 130 + 60 + 150 + 45, MaterialUnit.Gram).GetAwaiter().GetResult();

        wonderWomanProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        wonderWomanProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 4);
        wonderWomanProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 0.5);
        wonderWomanProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 1.5);

        ConsumeMaterial(wonderWomanProject, DatabaseSeed.MaterialMagnet10x5, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(wonderWomanProject, DatabaseSeed.MaterialMagnet8x4, 12, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(wonderWomanProject, DatabaseSeed.MaterialMagnet6x4, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(wonderWomanProject, DatabaseSeed.MaterialMagnet5x3, 4, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(wonderWomanProject, DatabaseSeed.MaterialVallejoPrimer, 200 + 325, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(wonderWomanProject, DatabaseSeed.MaterialArmyPainterAcrylic, 15 + 15 + 30 + 9 + 6 + 7 + 435, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(wonderWomanProject, DatabaseSeed.MaterialVallejoAcrylic, 000000000, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(wonderWomanProject, DatabaseSeed.MaterialAtomAcrylic, 000000000, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(wonderWomanProject, DatabaseSeed.MaterialVallejoVarnish, 000000000, MaterialUnit.Drop).GetAwaiter().GetResult();

        wonderWomanProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 25);

        context.Add(wonderWomanProject);

        context.SaveChanges();

        #endregion

        #region Black Widow

        var blackWidowsProject = new Project("rodrigo.basniak", "Black Widow", new DateTime(2025, 01, 01), modelId: null);

        blackWidowsProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (120) / 15.0);
        ConsumeMaterial(blackWidowsProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 150 + 60, MaterialUnit.Gram).GetAwaiter().GetResult();

        blackWidowsProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 0);
        blackWidowsProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 2);
        blackWidowsProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 0.25);
        blackWidowsProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 1);

        ConsumeMaterial(blackWidowsProject, DatabaseSeed.MaterialMagnet8x4, 10, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(blackWidowsProject, DatabaseSeed.MaterialMagnet6x4, 6, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(blackWidowsProject, DatabaseSeed.MaterialVallejoPrimer, 150 + 15 + 10, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(blackWidowsProject, DatabaseSeed.MaterialArmyPainterAcrylic, 15 + 5 + 5 + 5 + 30 + 30 + 10 + 50 + 10 + 30, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(blackWidowsProject, DatabaseSeed.MaterialVallejoVarnish, 60, MaterialUnit.Drop).GetAwaiter().GetResult();

        blackWidowsProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 0.5 + 1 + 1 + 1 + 4 + 9 + 2);

        context.Add(blackWidowsProject);

        context.SaveChanges();

        #endregion

        #region Lightning

        var lightningProject = new Project("rodrigo.basniak", "Lightning", new DateTime(2025, 01, 01), modelId: null);

        lightningProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (130 + 110) / 15.0);
        ConsumeMaterial(lightningProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 100 + 40 + 80 + 30, MaterialUnit.Gram).GetAwaiter().GetResult();

        lightningProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        lightningProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 4);
        lightningProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 0.5);
        lightningProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 2);

        ConsumeMaterial(lightningProject, DatabaseSeed.MaterialMagnet10x5, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(lightningProject, DatabaseSeed.MaterialMagnet8x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(lightningProject, DatabaseSeed.MaterialMagnet6x4, 8, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(lightningProject, DatabaseSeed.MaterialVallejoPrimer, 550, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(lightningProject, DatabaseSeed.MaterialArmyPainterAcrylic, 240 + 50 + 75 + 15 + 70, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(lightningProject, DatabaseSeed.MaterialVallejoVarnish, 100, MaterialUnit.Drop).GetAwaiter().GetResult();

        lightningProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 20);

        context.Add(lightningProject);

        context.SaveChanges();

        #endregion

        #region Thrall

        var thrallProject = new Project("rodrigo.basniak", "Thrall", new DateTime(2025, 01, 01), modelId: null);

        thrallProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (130 + 90 + 125) / 15.0);
        ConsumeMaterial(thrallProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 145 + 60 + 85 + 40 + 95 + 35, MaterialUnit.Gram).GetAwaiter().GetResult();

        thrallProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        thrallProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 6);
        thrallProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 1);
        thrallProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 2.5);

        ConsumeMaterial(thrallProject, DatabaseSeed.MaterialMagnet10x5, 16, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(thrallProject, DatabaseSeed.MaterialMagnet8x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(thrallProject, DatabaseSeed.MaterialVallejoPrimer, 450, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(thrallProject, DatabaseSeed.MaterialArmyPainterAcrylic, 30 + 15 + 30 + 10 + 10 + 15 + 5 + 25 + 15 + 25 + 10 + 25 + 20 + 40 + 30 + 10, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(thrallProject, DatabaseSeed.MaterialVallejoVarnish, 200, MaterialUnit.Drop).GetAwaiter().GetResult();

        thrallProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 1 + 1 + 3 + 2 + 2 + 6 + 4 + 2);

        context.Add(thrallProject);

        context.SaveChanges();

        #endregion

        #region Donkey Kong

        var DonkeyKongProject = new Project("rodrigo.basniak", "Donkey Kong", new DateTime(2025, 01, 01), modelId: null);

        DonkeyKongProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (85 + 175 + 100) / 15.0);
        ConsumeMaterial(DonkeyKongProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 110 + 30 + 250 + 90 + 70 + 20, MaterialUnit.Gram).GetAwaiter().GetResult();

        DonkeyKongProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        DonkeyKongProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 5);
        DonkeyKongProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 1);
        DonkeyKongProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 6);

        ConsumeMaterial(DonkeyKongProject, DatabaseSeed.MaterialMagnet6x4, 20, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(DonkeyKongProject, DatabaseSeed.MaterialMagnet8x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(DonkeyKongProject, DatabaseSeed.MaterialMagnet3x2, 4, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(DonkeyKongProject, DatabaseSeed.MaterialVallejoPrimer, 450, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(DonkeyKongProject, DatabaseSeed.MaterialArmyPainterAcrylic, 15 + 5 + 5 + 3 + 5 + 120 + 5 + 10 + 5 + 5 + 5 + 5 + 20 + 6 + 6 + 20 + 20 + 5 + 5 + 5 + 20 + 15 + 50 + 15 + 15 + 30 + 25 + 75, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(DonkeyKongProject, DatabaseSeed.MaterialVallejoVarnish, 200, MaterialUnit.Drop).GetAwaiter().GetResult();

        DonkeyKongProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 2.5 + 1 + 1 + 1.5 + 3 + 2 + 2 + 8 + 4);

        context.Add(DonkeyKongProject);

        context.SaveChanges();

        #endregion

        #region Final Fantasy Diorama

        var ffxivProject = new Project("rodrigo.basniak", "Final Fantasy Diorama", new DateTime(2025, 01, 01), modelId: null);

        ffxivProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (175 + 165 + 160 + 165 + 105 + 85) / 15.0);
        ConsumeMaterial(ffxivProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 140 + 80 + 190 + 45 + 175 + 55 + 170 + 55 + 85 + 35 + 95 + 40, MaterialUnit.Gram).GetAwaiter().GetResult();

        ffxivProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        ffxivProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 10);
        ffxivProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 1.5);
        ffxivProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 4);

        ConsumeMaterial(ffxivProject, DatabaseSeed.MaterialMagnet10x5, 12, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(ffxivProject, DatabaseSeed.MaterialMagnet8x4, 16, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(ffxivProject, DatabaseSeed.MaterialMagnet6x4, 15, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(ffxivProject, DatabaseSeed.MaterialVallejoPrimer, 400, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(ffxivProject, DatabaseSeed.MaterialArmyPainterAcrylic, 720, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(ffxivProject, DatabaseSeed.MaterialVallejoVarnish, 300, MaterialUnit.Drop).GetAwaiter().GetResult();

        ffxivProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 34);

        context.Add(ffxivProject);

        context.SaveChanges();

        #endregion

        #region Yunna

        var yunnaProject = new Project("rodrigo.basniak", "Yunna", new DateTime(2025, 01, 01), modelId: null);

        yunnaProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (100 + 150) / 15.0);
        ConsumeMaterial(yunnaProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 75 + 45 + 75 + 35, MaterialUnit.Gram).GetAwaiter().GetResult();

        yunnaProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        yunnaProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 4);
        yunnaProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 0.5);
        yunnaProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 2);

        ConsumeMaterial(yunnaProject, DatabaseSeed.MaterialMagnet10x5, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(yunnaProject, DatabaseSeed.MaterialMagnet8x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(yunnaProject, DatabaseSeed.MaterialMagnet6x4, 6, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(yunnaProject, DatabaseSeed.MaterialVallejoPrimer, 300, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(yunnaProject, DatabaseSeed.MaterialArmyPainterAcrylic, 250, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(yunnaProject, DatabaseSeed.MaterialVallejoVarnish, 150, MaterialUnit.Drop).GetAwaiter().GetResult();

        yunnaProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 23);

        context.Add(yunnaProject);

        context.SaveChanges();

        #endregion

        #region Makima

        var makimaProject = new Project("rodrigo.basniak", "Makima", new DateTime(2025, 01, 01), modelId: null);

        makimaProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (160) / 17);
        ConsumeMaterial(makimaProject, DatabaseSeed.MaterialJayoAbsResin, 100 + 60, MaterialUnit.Gram).GetAwaiter().GetResult();

        makimaProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 0.5);
        makimaProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 2);
        makimaProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 0.5);
        makimaProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 1);

        ConsumeMaterial(makimaProject, DatabaseSeed.MaterialMagnet10x5, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(makimaProject, DatabaseSeed.MaterialMagnet8x4, 2, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(makimaProject, DatabaseSeed.MaterialVallejoPrimer, 000000000, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(makimaProject, DatabaseSeed.MaterialArmyPainterAcrylic, 000000000, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(makimaProject, DatabaseSeed.MaterialVallejoVarnish, 000000000, MaterialUnit.Drop).GetAwaiter().GetResult();

        makimaProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 000000000);

        context.Add(makimaProject);

        context.SaveChanges();

        #endregion

        #region SAMPLE

        var lastOfUsProject = new Project("rodrigo.basniak", "Last Of Us", new DateTime(2026, 01, 23), modelId: null);

        // Two records for each project step
        lastOfUsProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2026, 01, 23), 1.5);
        lastOfUsProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2026, 01, 24), 1.0);
        lastOfUsProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2026, 01, 25), (160 + 145) / 15.0);
        lastOfUsProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2026, 01, 26), (130 + 120) / 15.0);
        lastOfUsProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2026, 01, 27), 3.0);
        lastOfUsProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2026, 01, 28), 2.0);
        lastOfUsProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2026, 01, 27), 1.0);
        lastOfUsProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2026, 01, 28), 0.75);
        lastOfUsProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2026, 01, 28), 2.25);
        lastOfUsProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2026, 01, 29), 1.5);
        lastOfUsProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2026, 01, 30), 6.0);
        lastOfUsProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2026, 02, 02), 5.5);

        // Materials: 2-3 from each category
        ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 420, MaterialUnit.Gram).GetAwaiter().GetResult();
        ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialJayoAbsResin, 210, MaterialUnit.Gram).GetAwaiter().GetResult();

        ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialMagnet10x5, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialMagnet8x4, 10, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialMagnet6x4, 6, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialVallejoPrimer, 160, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialGswPrimer, 120, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialArmyPainterAcrylic, 260, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialVallejoAcrylic, 120, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialAtomAcrylic, 75, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialVallejoVarnish, 90, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 35, MaterialUnit.Spray).GetAwaiter().GetResult();

        ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialMaskingTapeGeneric10mm, 120, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialMaskingTapeTamiya10mm, 90, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialVallejoLiquidMask, 12, MaterialUnit.Mililiter).GetAwaiter().GetResult();

        ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialDisposableBrush, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialPaintMixingCupCorrugated, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialNailWoodSwab, 30, MaterialUnit.Unit).GetAwaiter().GetResult();

        context.Add(lastOfUsProject);

        // Finished pictures (>= 3)
        var sampleFinishedPictures = new[]
        {
            "uploads\\sample\\last_of_us\\project_01.png",
            "uploads\\sample\\last_of_us\\project_02.png",
            "uploads\\sample\\last_of_us\\project_03.png"
        };
        foreach (var x in sampleFinishedPictures)
        {
            lastOfUsProject.AddFinishedPicture(x);
        }

        // Reference pictures (>= 3)
        var sampleReferencePictures = new[]
        {
            "uploads\\sample\\last_of_us\\reference_01.png",
            "uploads\\sample\\last_of_us\\reference_02.png",
            "uploads\\sample\\last_of_us\\reference_03.png"
        };
        foreach (var x in sampleReferencePictures)
        {
            lastOfUsProject.AddReferencePicture(x);
        }

        List<PaintingProjectsManagement.Features.Inventory.Integration.ColorMatchResult> FindPaintMatches(string referenceColor, int maxResults = 5)
        {
            var userPaints = context.Set<PaintingProjectsManagement.Features.Inventory.UserPaint>()
                .Where(x => x.Username.ToLower() == "rodrigo.basniak")
                .Include(x => x.PaintColor)
                .ThenInclude(x => x.Line)
                .ThenInclude(x => x.Brand)
                .ToList();

            return userPaints
                .Select(x => new PaintingProjectsManagement.Features.Inventory.Integration.ColorMatchResult
                {
                    PaintColorId = x.PaintColorId,
                    Name = x.PaintColor.Name,
                    HexColor = x.PaintColor.HexColor,
                    BrandName = x.PaintColor.Line.Brand.Name,
                    LineName = x.PaintColor.Line.Name,
                    Distance = PaintingProjectsManagement.Features.Inventory.ColorHelper.CalculateColorDistance(referenceColor, x.PaintColor.HexColor)
                })
                .OrderBy(x => x.Distance)
                .ThenBy(x => x.Name)
                .Take(maxResults)
                .ToList();
        }

        void AddSectionWithMatches(ColorGroup colorGroup, ColorZone zone, string referenceColor, bool includeMatches = true)
        {
            colorGroup.AddSection(zone, referenceColor);
            var colorSection = colorGroup.Sections.First(x => x.Zone == zone);

            if (includeMatches)
            {
                var matches = FindPaintMatches(referenceColor);
                if (matches.Count > 0)
                {
                    colorSection.UpdateSuggestedColors(matches);
                    colorSection.SetPickedColor(matches[0].PaintColorId);
                }
            }
        }

        void AddColorGroup(Project project, string name, params (ColorZone Zone, string ReferenceColor, bool IncludeMatches)[] sections)
        {
            var colorGroup = new ColorGroup(project, name);
            context.Add(colorGroup);
            project.AddColorGroup(colorGroup);

            foreach (var x in sections)
            {
                AddSectionWithMatches(colorGroup, x.Zone, x.ReferenceColor, x.IncludeMatches);
            }
        }

        // >= 5 zones/groups, with mixed complexity (some triads, some partial sections)
        AddColorGroup(
            lastOfUsProject,
            "Skin",
            (ColorZone.Midtone, "#C29681", true),
            (ColorZone.Highlight, "#E0B59D", true),
            (ColorZone.Shadow, "#8C5F4F", true));

        AddColorGroup(
            lastOfUsProject,
            "Jacket Leather",
            (ColorZone.Midtone, "#6D4A33", true),
            (ColorZone.Highlight, "#9A6E4D", true),
            (ColorZone.Shadow, "#3A2A20", true));

        AddColorGroup(
            lastOfUsProject,
            "Jeans Denim",
            (ColorZone.Midtone, "#3D5E7A", true),
            (ColorZone.Highlight, "#6D8FAE", true),
            (ColorZone.Shadow, "#24384A", true));

        AddColorGroup(
            lastOfUsProject,
            "Backpack Canvas",
            (ColorZone.Midtone, "#5B6E4A", true),
            (ColorZone.Shadow, "#35422A", true));

        AddColorGroup(
            lastOfUsProject,
            "Metal Details",
            (ColorZone.Midtone, "#7A7F87", true),
            (ColorZone.Highlight, "#B6BCC4", true));

        AddColorGroup(
            lastOfUsProject,
            "Base Vegetation",
            (ColorZone.Midtone, "#4E603A", false));

        context.SaveChanges();

        #endregion

        //#region SAMPLE 

        //var newProject = new Project("rodrigo.basniak", "New_Project", new DateTime(2025, 01, 01), modelId: null);

        //newProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (000000000) / 15.0);
        //ConsumeMaterial(newProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 000000000, MaterialUnit.Gram).GetAwaiter().GetResult();

        //newProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        //newProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 000000000);
        //newProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 000000000);
        //newProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 000000000);

        //ConsumeMaterial(newProject, DatabaseSeed.MaterialMagnet10x5, 000000000, MaterialUnit.Unit).GetAwaiter().GetResult();
        //ConsumeMaterial(newProject, DatabaseSeed.MaterialMagnet8x4, 000000000, MaterialUnit.Unit).GetAwaiter().GetResult();
        //ConsumeMaterial(newProject, DatabaseSeed.MaterialMagnet6x4, 000000000, MaterialUnit.Unit).GetAwaiter().GetResult();

        //ConsumeMaterial(newProject, DatabaseSeed.MaterialVallejoPrimer, 000000000, MaterialUnit.Drop).GetAwaiter().GetResult();

        //ConsumeMaterial(newProject, DatabaseSeed.MaterialArmyPainterAcrylic, 000000000, MaterialUnit.Drop).GetAwaiter().GetResult();
        //ConsumeMaterial(newProject, DatabaseSeed.MaterialVallejoAcrylic, 000000000, MaterialUnit.Drop).GetAwaiter().GetResult();
        //ConsumeMaterial(newProject, DatabaseSeed.MaterialAtomAcrylic, 000000000, MaterialUnit.Drop).GetAwaiter().GetResult();

        //ConsumeMaterial(newProject, DatabaseSeed.MaterialVallejoVarnish, 000000000, MaterialUnit.Drop).GetAwaiter().GetResult();

        //newProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 000000000);

        //context.Add(newProject);

        //context.SaveChanges();

        //#endregion
    }
}
