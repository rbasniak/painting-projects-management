using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Features.Projects;
using rbkApiModules.Commons.Relational;
using System.Diagnostics;

namespace PaintingProjectsManagment.Database;

public class ProjectsSeed : IDeferredSeedStep
{
    public string Id => "2026-04-13 22h35m: Seed initial projects data";

    public EnvironmentUsage EnvironmentUsage => EnvironmentUsage.Production | EnvironmentUsage.Development | EnvironmentUsage.Staging | EnvironmentUsage.Testing;

    public Type DbContextType => typeof(DatabaseContext);

    public async Task ExecuteAsync(DbContext context, IServiceProvider serviceProvider)
    {
        async Task ConsumeMaterial(Project project, string materialName, double quantity, MaterialUnit unit)
        {
            Material? material = null;

            var timeout = DateTime.UtcNow.AddSeconds(60);

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

            if (string.IsNullOrWhiteSpace(material.Tenant))
            {
                context.Database.ExecuteSqlRaw("update public.\"projects.projections.materials\" set \"Tenant\" = 'RODRIGO.BASNIAK' WHERE \"Tenant\" IS NULL OR \"Tenant\" = ''");
            }

            project.ConsumeMaterial(material.Id, quantity, unit);
        }

        #region Archangel

        var archangelProject = new Project("rodrigo.basniak", "Archangel", new DateTime(2025, 07, 15), modelId: null);

        await ConsumeMaterial(archangelProject, DatabaseSeed.MaterialMagnet10x5, 6, MaterialUnit.Unit);
        await ConsumeMaterial(archangelProject, DatabaseSeed.MaterialMagnet5x3, 3, MaterialUnit.Unit);
        await ConsumeMaterial(archangelProject, DatabaseSeed.MaterialMagnet3x2, 6, MaterialUnit.Unit);
        await ConsumeMaterial(archangelProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 165 + 120 + 75 + 25 + 95 + 60, MaterialUnit.Gram);
        await ConsumeMaterial(archangelProject, DatabaseSeed.MaterialVallejoPrimer, 200, MaterialUnit.Drop);
        await ConsumeMaterial(archangelProject, DatabaseSeed.MaterialGswPrimer, 200, MaterialUnit.Drop);
        await ConsumeMaterial(archangelProject, DatabaseSeed.MaterialGswChrome, 375 + 50, MaterialUnit.Drop);
        await ConsumeMaterial(archangelProject, DatabaseSeed.MaterialArmyPainterAcrylic, 50 + 25 + 10 + 10 + 20 + 10 + 10 + 15 + 5 + 10 + 5 + 5 + 25 + 25 + 50, MaterialUnit.Drop);

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

        await ConsumeMaterial(illidanProject, DatabaseSeed.MaterialMagnet10x5, 6, MaterialUnit.Unit);
        await ConsumeMaterial(illidanProject, DatabaseSeed.MaterialMagnet8x4, 18, MaterialUnit.Unit);
        await ConsumeMaterial(illidanProject, DatabaseSeed.MaterialMagnet3x2, 8, MaterialUnit.Unit);

        await ConsumeMaterial(illidanProject, DatabaseSeed.MaterialPaintMixingCupCorrugated, 10, MaterialUnit.Unit);
        await ConsumeMaterial(illidanProject, DatabaseSeed.MaterialMaskingTape3m25mm, 3, MaterialUnit.Meter);
        await ConsumeMaterial(illidanProject, DatabaseSeed.MaterialVallejoLiquidMask, 2, MaterialUnit.Mililiter);
        await ConsumeMaterial(illidanProject, DatabaseSeed.MaterialDisposableBrush, 2, MaterialUnit.Unit);

        await ConsumeMaterial(illidanProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, (210 + 160 + 205), MaterialUnit.Gram);

        await ConsumeMaterial(illidanProject, DatabaseSeed.MaterialVallejoPrimer, 150, MaterialUnit.Drop);
        await ConsumeMaterial(illidanProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 68 + 19 + 30, MaterialUnit.Spray);
        await ConsumeMaterial(illidanProject, DatabaseSeed.MaterialGswVarnish, 50, MaterialUnit.Drop);

        await ConsumeMaterial(illidanProject, DatabaseSeed.MaterialArmyPainterAcrylic, 345 + 50 + 25 + 115 + 50 + 75, MaterialUnit.Drop);

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

        await ConsumeMaterial(sylvanasProject, DatabaseSeed.MaterialMagnet10x5, 6, MaterialUnit.Unit);
        await ConsumeMaterial(sylvanasProject, DatabaseSeed.MaterialMagnet8x4, 16, MaterialUnit.Unit);
        await ConsumeMaterial(sylvanasProject, DatabaseSeed.MaterialMagnet6x4, 10, MaterialUnit.Unit);
        await ConsumeMaterial(sylvanasProject, DatabaseSeed.MaterialMagnet3x2, 8, MaterialUnit.Unit);

        await ConsumeMaterial(sylvanasProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, (200 + 235 + 150), MaterialUnit.Gram);

        await ConsumeMaterial(sylvanasProject, DatabaseSeed.MaterialVallejoPrimer, 225, MaterialUnit.Drop);
        await ConsumeMaterial(sylvanasProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 75, MaterialUnit.Spray);
        await ConsumeMaterial(sylvanasProject, DatabaseSeed.MaterialGswAcrylic, 125 + 50 + 5 + 15, MaterialUnit.Drop);
        await ConsumeMaterial(sylvanasProject, DatabaseSeed.MaterialGswChrome, 100, MaterialUnit.Drop);
        await ConsumeMaterial(sylvanasProject, DatabaseSeed.MaterialTamiyaTS80, 15, MaterialUnit.Spray);
        await ConsumeMaterial(sylvanasProject, DatabaseSeed.MaterialArmyPainterAcrylic, 10 + 20 + 50 + 10 + 15 + 25 + 50 + 50 + 10, MaterialUnit.Drop);

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

        await ConsumeMaterial(laraProject, DatabaseSeed.MaterialMagnet10x5, 6, MaterialUnit.Unit);
        await ConsumeMaterial(laraProject, DatabaseSeed.MaterialMagnet8x4, 14, MaterialUnit.Unit);
        await ConsumeMaterial(laraProject, DatabaseSeed.MaterialMagnet6x4, 4, MaterialUnit.Unit);

        await ConsumeMaterial(laraProject, DatabaseSeed.MaterialTamiyaTS80, 70 + 5 + 15 + 50, MaterialUnit.Spray);

        await ConsumeMaterial(laraProject, DatabaseSeed.MaterialMaskingTapeTamiya10mm, 30, MaterialUnit.Centimeter);

        laraProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (105.0 + 177.0 + 186.0) / 20.0);
        await ConsumeMaterial(laraProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, (180.0 + 165.5 + 195.0), MaterialUnit.Gram);

        await ConsumeMaterial(laraProject, DatabaseSeed.MaterialVallejoPrimer, 175, MaterialUnit.Drop);
        await ConsumeMaterial(laraProject, DatabaseSeed.MaterialArmyPainterAcrylic, 25 + 95 + 150 + 30 + 75 + 50 + 40 + 155, MaterialUnit.Drop);

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
        await ConsumeMaterial(camusProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, (175 + 125 + 335 + 60 + 150 + 45 + 160 + 50 + 130 + 75), MaterialUnit.Gram);

        await ConsumeMaterial(camusProject, DatabaseSeed.MaterialMagnet10x5, 4, MaterialUnit.Unit);
        await ConsumeMaterial(camusProject, DatabaseSeed.MaterialMagnet8x4, 6, MaterialUnit.Unit);
        await ConsumeMaterial(camusProject, DatabaseSeed.MaterialMagnet6x4, 10, MaterialUnit.Unit);

        await ConsumeMaterial(camusProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 92 + 65 + 35, MaterialUnit.Spray);

        await ConsumeMaterial(camusProject, DatabaseSeed.MaterialVallejoPrimer, 100 + 100 + 100, MaterialUnit.Drop);
        await ConsumeMaterial(camusProject, DatabaseSeed.MaterialArmyPainterAcrylic, 100 + 75 + 15 + 25 + 25 + 125 + 50 + 50 + 25, MaterialUnit.Drop);

        camusProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 4);
        camusProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 1.5);
        camusProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 4.5);
        camusProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 1 + 3 + 2 + 2 + 1.5 + 2 + 4);

        context.Add(camusProject);

        context.SaveChanges();

        #endregion

        #region Kratos

        var kratosProject = new Project("rodrigo.basniak", "Kratos", new DateTime(2025, 08, 15), modelId: null);

        await ConsumeMaterial(kratosProject, DatabaseSeed.MaterialMagnet10x5, 34, MaterialUnit.Unit);
        await ConsumeMaterial(kratosProject, DatabaseSeed.MaterialMagnet8x4, 9, MaterialUnit.Unit);
        await ConsumeMaterial(kratosProject, DatabaseSeed.MaterialMagnet6x4, 8, MaterialUnit.Unit);
        await ConsumeMaterial(kratosProject, DatabaseSeed.MaterialDspiaeSandingDisk, 4, MaterialUnit.Unit);
        await ConsumeMaterial(kratosProject, DatabaseSeed.MaterialModellingPutty, 5, MaterialUnit.Gram);
        await ConsumeMaterial(kratosProject, DatabaseSeed.MaterialMaskingTapeGeneric5mm, 1, MaterialUnit.Meter);
        await ConsumeMaterial(kratosProject, DatabaseSeed.MaterialDisposableBrush, 5, MaterialUnit.Unit);
        await ConsumeMaterial(kratosProject, DatabaseSeed.MaterialPaintMixingCupCorrugated, 10, MaterialUnit.Unit);
        await ConsumeMaterial(kratosProject, DatabaseSeed.MaterialPaintMixingCupNonCorrugated, 5, MaterialUnit.Unit);
        await ConsumeMaterial(kratosProject, DatabaseSeed.MaterialMaskingTapeGeneric10mm, 200, MaterialUnit.Centimeter);
        await ConsumeMaterial(kratosProject, DatabaseSeed.MaterialMaskingTape3m25mm, 100, MaterialUnit.Centimeter);
        await ConsumeMaterial(kratosProject, DatabaseSeed.MaterialArmyPainterSnow, 150 * 0.75, MaterialUnit.Gram);
        await ConsumeMaterial(kratosProject, DatabaseSeed.MaterialMaskingTapeTamiya20mm, 100, MaterialUnit.Centimeter);

        kratosProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (160 + 160 + 155 + 155 + 135 + 125 + 190) / 20.0);
        await ConsumeMaterial(kratosProject, DatabaseSeed.MaterialSunluAbsResin, 195 + 75 + 225 + 65 + 195 + 70 + 135 + 55 + 165 + 60 + 155 + 60 + 150 + 80, MaterialUnit.Gram);

        await ConsumeMaterial(kratosProject, DatabaseSeed.MaterialVallejoPrimer, 175 + 375 + 50 + 15, MaterialUnit.Drop);
        await ConsumeMaterial(kratosProject, DatabaseSeed.MaterialVallejoVarnish, 500 + 25 + 25, MaterialUnit.Drop);
        await ConsumeMaterial(kratosProject, DatabaseSeed.MaterialGswVarnish, 25, MaterialUnit.Drop);
        await ConsumeMaterial(kratosProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 110 + 40 + 105, MaterialUnit.Spray);

        await ConsumeMaterial(kratosProject, DatabaseSeed.MaterialArmyPainterAcrylic, 245 + 640, MaterialUnit.Drop);

        kratosProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 9.5);
        kratosProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 2);
        kratosProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 9);
        kratosProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 1.75 + 4.5 + 2 + 4 + 10 + 10 + 8 + 7.5 + 5);

        context.Add(kratosProject);

        context.SaveChanges();

        #endregion

        #region Starlight

        var starligtProject = new Project("rodrigo.basniak", "Starlight", new DateTime(2025, 08, 15), modelId: null);

        await ConsumeMaterial(starligtProject, DatabaseSeed.MaterialMagnet10x5, 4, MaterialUnit.Unit);
        await ConsumeMaterial(starligtProject, DatabaseSeed.MaterialMagnet8x4, 6, MaterialUnit.Unit);
        await ConsumeMaterial(starligtProject, DatabaseSeed.MaterialMagnet6x4, 6, MaterialUnit.Unit);
        await ConsumeMaterial(starligtProject, DatabaseSeed.MaterialMagnet3x2, 4, MaterialUnit.Unit);

        await ConsumeMaterial(starligtProject, DatabaseSeed.MaterialTamiyaTS80, 25 + 25 + 25 + 20, MaterialUnit.Spray);

        await ConsumeMaterial(starligtProject, DatabaseSeed.MaterialDisposableBrush, 2, MaterialUnit.Unit);

        starligtProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (115.0 + 85.0) / 20.0);
        await ConsumeMaterial(starligtProject, DatabaseSeed.MaterialJayoAbsResin, 150 + 90 + 40, MaterialUnit.Gram);

        await ConsumeMaterial(starligtProject, DatabaseSeed.MaterialVallejoPrimer, 50, MaterialUnit.Drop);
        await ConsumeMaterial(starligtProject, DatabaseSeed.MaterialArmyPainterAcrylic, 50 + 50 + 50 + 20 + 25 + 40 + 5 + 20 + 145 + 75 + 10 + 25 + 10, MaterialUnit.Drop);

        starligtProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 2.5);
        starligtProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 1.5);
        starligtProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 1);
        starligtProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 0.5 + 1 + 3 + 1.5 + 2 + 2);

        context.Add(starligtProject);

        context.SaveChanges();

        #endregion

        #region Harley Quinn 

        var harleyProject = new Project("rodrigo.basniak", "Harley Quinn", new DateTime(2025, 08, 15), modelId: null);

        await ConsumeMaterial(harleyProject, DatabaseSeed.MaterialMagnet10x5, 8, MaterialUnit.Unit);
        await ConsumeMaterial(harleyProject, DatabaseSeed.MaterialMagnet8x4, 4, MaterialUnit.Unit);
        await ConsumeMaterial(harleyProject, DatabaseSeed.MaterialMagnet6x4, 6, MaterialUnit.Unit);

        await ConsumeMaterial(harleyProject, DatabaseSeed.MaterialVallejoPrimer, 150, MaterialUnit.Drop);
        await ConsumeMaterial(harleyProject, DatabaseSeed.MaterialArmyPainterAcrylic, 40 + 75 + 40 + 425 + 45, MaterialUnit.Drop);

        await ConsumeMaterial(harleyProject, DatabaseSeed.MaterialTamiyaTS80, 125 + 30 + 35 + 45 + 45, MaterialUnit.Spray);
        await ConsumeMaterial(harleyProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 25, MaterialUnit.Spray);

        await ConsumeMaterial(harleyProject, DatabaseSeed.MaterialDisposableBrush, 2, MaterialUnit.Unit);

        await ConsumeMaterial(harleyProject, DatabaseSeed.MaterialMaskingTape3m25mm, 60, MaterialUnit.Centimeter);
        await ConsumeMaterial(harleyProject, DatabaseSeed.MaterialMaskingTapeGeneric25mm, 40, MaterialUnit.Centimeter);

        harleyProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (152 + 105) / 20.0);
        await ConsumeMaterial(harleyProject, DatabaseSeed.MaterialSunluAbsResin, 235 + 115, MaterialUnit.Gram);


        harleyProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 3);
        harleyProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 0.5);
        harleyProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 2);
        harleyProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 1 + 2.5 + 8 + 9 + 2.5 + 6 + 2.5 + 1 + 1.5);

        context.Add(harleyProject);

        context.SaveChanges();

        #endregion

        #region Sadie Adler

        var sadieProject = new Project("rodrigo.basniak", "Sadie Adler", new DateTime(2025, 08, 15), modelId: null);

        await ConsumeMaterial(sadieProject, DatabaseSeed.MaterialMagnet10x5, 4, MaterialUnit.Unit);
        await ConsumeMaterial(sadieProject, DatabaseSeed.MaterialMagnet8x4, 4, MaterialUnit.Unit);
        await ConsumeMaterial(sadieProject, DatabaseSeed.MaterialMagnet6x4, 4, MaterialUnit.Unit);
        await ConsumeMaterial(sadieProject, DatabaseSeed.MaterialMagnet5x3, 4, MaterialUnit.Unit);

        await ConsumeMaterial(sadieProject, DatabaseSeed.MaterialVallejoPrimer, 150, MaterialUnit.Drop);
        await ConsumeMaterial(sadieProject, DatabaseSeed.MaterialArmyPainterAcrylic, 10 + 30 + 90 + 5 + 3 + 15 + 20 + 20 + 5 + 15 + 5 + 15, MaterialUnit.Drop);
        await ConsumeMaterial(sadieProject, DatabaseSeed.MaterialVallejoAcrylic, 45, MaterialUnit.Drop);
        await ConsumeMaterial(sadieProject, DatabaseSeed.MaterialAtomAcrylic, 10 + 7 + 25 + 2 + 5 + 20 + 30, MaterialUnit.Drop);

        await ConsumeMaterial(sadieProject, DatabaseSeed.MaterialDisposableBrush, 1, MaterialUnit.Unit);
        await ConsumeMaterial(sadieProject, DatabaseSeed.MaterialDspiaeSandingDisk, 1, MaterialUnit.Unit);

        await ConsumeMaterial(sadieProject, DatabaseSeed.MaterialMaskingTapeTamiya10mm, 20, MaterialUnit.Centimeter);
        await ConsumeMaterial(sadieProject, DatabaseSeed.MaterialMaskingTapeGeneric25mm, 20, MaterialUnit.Centimeter);
        await ConsumeMaterial(sadieProject, DatabaseSeed.MaterialMaskingTapeGeneric10mm, 20 + 15, MaterialUnit.Centimeter);

        sadieProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (70 + 150) / 20.0);
        await ConsumeMaterial(sadieProject, DatabaseSeed.MaterialJayoAbsResin, (75 + 35 + 100 + 45), MaterialUnit.Gram);

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
        await ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialJayoAbsResin, (185 + 175 + 220 + 160 + 175 + 165 + 200 + 185 + 180 + 185 + 215 + 210 + 170 + 235 + 95 + 105), MaterialUnit.Gram);

        await ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialMagnet10x5, 10, MaterialUnit.Unit);
        await ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialMagnet8x4, 4, MaterialUnit.Unit);
        await ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialDspiaeSandingDisk, 6, MaterialUnit.Unit);
        await ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialDisposableBrush, 10, MaterialUnit.Unit);
        await ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialPaintMixingCupCorrugated, 4, MaterialUnit.Unit);
        await ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialGlooves, 25, MaterialUnit.Unit);
        await ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialNailWoodSwab, 50, MaterialUnit.Unit);

        await ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialGswPrimer, 1040, MaterialUnit.Drop);
        await ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialVallejoPrimer, 215, MaterialUnit.Drop);
        await ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialAkChrome, 115, MaterialUnit.Drop);
        await ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialGswChrome, 50 + 100 + 100 + 100 + 39 + 100 + 100 + 100 + 75, MaterialUnit.Drop);
        await ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialVallejoAcrylic, 10, MaterialUnit.Drop);
        await ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialTamiyaClear, 75 + 50 + 40 + 200 + 6 + 20 + 250 + 250, MaterialUnit.Drop);
        await ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialAmmoCandy, 75 + 10, MaterialUnit.Drop);
        await ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialArmyPainterAcrylic, 45 + 50 + 40 + 10 + 30 + 50 + 20 + 10 + 10 + 50 + 75 + 10 + 20, MaterialUnit.Drop);
        await ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialGswVarnish, 200 + 100 + 50 + 75, MaterialUnit.Drop);
        await ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialAmmoTransparator, 20, MaterialUnit.Drop);
        await ConsumeMaterial(hulkbusterProject, DatabaseSeed.MaterialAmmoDrybrush, 50, MaterialUnit.Drop);


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
        await ConsumeMaterial(harryProject, DatabaseSeed.MaterialJayoAbsResin, 115 + 30 + 185 + 55 + 51 + 25 + 190 + 85, MaterialUnit.Gram);

        harryProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 12), 1);
        harryProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 12), 5);
        harryProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 12), 2);
        harryProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 12), 1.5);

        await ConsumeMaterial(harryProject, DatabaseSeed.MaterialMagnet10x5, 8, MaterialUnit.Unit);
        await ConsumeMaterial(harryProject, DatabaseSeed.MaterialMagnet8x4, 6, MaterialUnit.Unit);
        await ConsumeMaterial(harryProject, DatabaseSeed.MaterialMaskingTapeTamiya10mm, 10 + 15, MaterialUnit.Centimeter);
        await ConsumeMaterial(harryProject, DatabaseSeed.MaterialMaskingTapeGeneric25mm, 15 + 25, MaterialUnit.Centimeter);
        await ConsumeMaterial(harryProject, DatabaseSeed.MaterialMaskingTapeGeneric10mm, 100, MaterialUnit.Centimeter);

        await ConsumeMaterial(harryProject, DatabaseSeed.MaterialVallejoPrimer, 300, MaterialUnit.Drop);
        await ConsumeMaterial(harryProject, DatabaseSeed.MaterialGswChrome, 30, MaterialUnit.Drop);
        await ConsumeMaterial(harryProject, DatabaseSeed.MaterialArmyPainterAcrylic, 100 + 25 + 50 + 5 + 25 + 20 + 25 + 5 + 5 + 5 + 5 + 5 + 5 + 5 + 5 + 10, MaterialUnit.Drop);
        await ConsumeMaterial(harryProject, DatabaseSeed.MaterialVallejoAcrylic, 5 + 15 + 10 + 5 + 25 + 10 + 15, MaterialUnit.Drop);
        await ConsumeMaterial(harryProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 50, MaterialUnit.Spray);
        await ConsumeMaterial(harryProject, DatabaseSeed.MaterialAtomAcrylic, 30 + 10 + 5 + 4 + 7, MaterialUnit.Drop);
        await ConsumeMaterial(harryProject, DatabaseSeed.MaterialAmmoAcrylic, 5 + 40 + 10 + 10, MaterialUnit.Drop);
        await ConsumeMaterial(harryProject, DatabaseSeed.MaterialVallejoVarnish, 30 + 55, MaterialUnit.Drop);
        await ConsumeMaterial(harryProject, DatabaseSeed.MaterialAmmoDrybrush, 5, MaterialUnit.Drop);
        await ConsumeMaterial(harryProject, DatabaseSeed.MaterialAmmoTransparator, 130 + 40 + 10 + 15, MaterialUnit.Drop);

        harryProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 12), 0.5 + 5 + 1 + 1.5 + 6.5 + 5 + 1);

        context.Add(harryProject);

        context.SaveChanges();

        #endregion

        #region Raven

        var ravenProject = new Project("rodrigo.basniak", "Raven", new DateTime(2025, 11, 01), modelId: null);

        ravenProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (190 + 210) / 20);
        await ConsumeMaterial(ravenProject, DatabaseSeed.MaterialJayoAbsResin, (170 + 55 + 195 + 50), MaterialUnit.Gram);

        ravenProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        ravenProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 5);
        ravenProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 1);
        ravenProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 2);

        await ConsumeMaterial(ravenProject, DatabaseSeed.MaterialMagnet10x5, 8, MaterialUnit.Unit);
        await ConsumeMaterial(ravenProject, DatabaseSeed.MaterialMagnet8x4, 4, MaterialUnit.Unit);
        await ConsumeMaterial(ravenProject, DatabaseSeed.MaterialMagnet4x3, 2, MaterialUnit.Unit);
        await ConsumeMaterial(ravenProject, DatabaseSeed.MaterialMagnet3x2, 1, MaterialUnit.Unit);
        await ConsumeMaterial(ravenProject, DatabaseSeed.MaterialMagnet6x4, 2, MaterialUnit.Unit);

        await ConsumeMaterial(ravenProject, DatabaseSeed.MaterialMaskingTape3m25mm, 15, MaterialUnit.Centimeter);
        await ConsumeMaterial(ravenProject, DatabaseSeed.MaterialMaskingTapeGeneric25mm, 10, MaterialUnit.Centimeter);
        await ConsumeMaterial(ravenProject, DatabaseSeed.MaterialMaskingTapeTamiya10mm, 30, MaterialUnit.Centimeter);
        await ConsumeMaterial(ravenProject, DatabaseSeed.MaterialDisposableBrush, 2, MaterialUnit.Unit);
        await ConsumeMaterial(ravenProject, DatabaseSeed.MaterialNailWoodSwab, 35, MaterialUnit.Unit);

        await ConsumeMaterial(ravenProject, DatabaseSeed.MaterialVallejoPrimer, 250, MaterialUnit.Drop);
        await ConsumeMaterial(ravenProject, DatabaseSeed.MaterialArmyPainterAcrylic, 25 + 10 + 5 + 50 + 30 + 20 + 15 + 15 + 20, MaterialUnit.Drop);
        await ConsumeMaterial(ravenProject, DatabaseSeed.MaterialVallejoAcrylic, 5 + 10 + 10, MaterialUnit.Drop);
        await ConsumeMaterial(ravenProject, DatabaseSeed.MaterialAmmoDrybrush, 5, MaterialUnit.Drop);
        await ConsumeMaterial(ravenProject, DatabaseSeed.MaterialAtomAcrylic, 18, MaterialUnit.Drop);
        await ConsumeMaterial(ravenProject, DatabaseSeed.MaterialAmmoTransparator, 30, MaterialUnit.Drop);
        await ConsumeMaterial(ravenProject, DatabaseSeed.MaterialTamiyaTS80, 55, MaterialUnit.Spray);
        await ConsumeMaterial(ravenProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 25, MaterialUnit.Spray);
        await ConsumeMaterial(ravenProject, DatabaseSeed.MaterialVallejoVarnish, 15, MaterialUnit.Drop);

        ravenProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 3 + 4 + 6 + 0.5);

        context.Add(ravenProject);

        context.SaveChanges();

        #endregion

        #region Lace

        var laceProject = new Project("rodrigo.basniak", "Lady Lace", new DateTime(2025, 11, 01), modelId: null);

        laceProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (200 + 30) / 20);
        await ConsumeMaterial(laceProject, DatabaseSeed.MaterialJayoAbsResin, (200 + 40), MaterialUnit.Gram);

        laceProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        laceProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 1.5);
        laceProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 0.5);
        laceProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 0);

        await ConsumeMaterial(laceProject, DatabaseSeed.MaterialMagnet10x5, 8, MaterialUnit.Unit);

        await ConsumeMaterial(laceProject, DatabaseSeed.MaterialVallejoPrimer, 250, MaterialUnit.Drop);

        //ConsumeMaterial(laceProject, DatabaseSeed.MaterialArmyPainterAcrylic, 245 + 640, MaterialUnit.Drop);
        //ConsumeMaterial(laceProject, DatabaseSeed.MaterialArmyPainterAcrylic, 245 + 640, MaterialUnit.Drop);
        //ConsumeMaterial(laceProject, DatabaseSeed.MaterialArmyPainterAcrylic, 245 + 640, MaterialUnit.Drop);
        //ConsumeMaterial(laceProject, DatabaseSeed.MaterialArmyPainterAcrylic, 245 + 640, MaterialUnit.Drop);

        // laceProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 0.0);

        context.Add(laceProject);

        context.SaveChanges();

        #endregion

        #region Aphrodite

        var aphroditeProject = new Project("rodrigo.basniak", "Aphrodite of Pisces", new DateTime(2025, 12, 10), modelId: null);

        aphroditeProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (90 + 210 + 215 + 210 + 120 + 205 + 200 + 155 + 140) / 17);
        await ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialJayoAbsResin, 110 + 50 + 160 + 70 + 220 + 105 + 160 + 105, MaterialUnit.Gram);
        await ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialAnycubricClearResin, 70 + 70 + 90 + 50 + 180 + 85 + 115 + 45 + 85 + 55, MaterialUnit.Gram);

        aphroditeProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 2);
        aphroditeProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 18);
        aphroditeProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 4.5);
        aphroditeProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 3);

        await ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialMagnet10x5, 12, MaterialUnit.Unit);
        await ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialMagnet8x4, 7, MaterialUnit.Unit);
        await ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialMagnet6x4, 6, MaterialUnit.Unit);

        await ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialVallejoPrimer, 125 + 30 + 35 + 30 + 15, MaterialUnit.Drop);
        await ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialGswPrimer, 350, MaterialUnit.Drop);
        await ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialTamiyaClear, 180 + 60 + 20 + 50 + 10, MaterialUnit.Drop);
        await ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialAmmoCandy, 40 + 80 + 10 + 30, MaterialUnit.Drop);
        await ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialArmyPainterAcrylic, 20 + 60 + 10 + 25 + 40 + 10, MaterialUnit.Drop);
        await ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialGswVarnish, 50, MaterialUnit.Drop);
        await ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialAtomAcrylic, 30, MaterialUnit.Drop);
        await ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialGswChrome, 220, MaterialUnit.Drop);
        await ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialAmmoDrybrush, 50, MaterialUnit.Drop);
        await ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialGswAcrylic, 30, MaterialUnit.Drop);

        await ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialBiltemaGlossVarnish, 30 + 70 + 30 + 67 + 35 + 55 + 40 + 55, MaterialUnit.Spray);
        await ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialTamiyaTS80, 40, MaterialUnit.Spray);

        await ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialPaintMixingCupCorrugated, 3, MaterialUnit.Unit);
        await ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialNailWoodSwab, 40, MaterialUnit.Unit);
        await ConsumeMaterial(aphroditeProject, DatabaseSeed.MaterialDisposableBrush, 10, MaterialUnit.Unit);

        aphroditeProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 12, 12), 1.5 + 0.5 + 1 + 1 + 3 + 2 + 2 + 1.5 + 1 + 3 + 1 + 2 + 1);

        context.Add(aphroditeProject);

        context.SaveChanges();

        #endregion

        #region Iron Man and Tony Stark

        var tonyStarkProject = new Project("rodrigo.basniak", "Iton Man and Tony Stark", new DateTime(2025, 01, 01), modelId: null);

        tonyStarkProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (200 + 125 + 90) / 13);
        await ConsumeMaterial(tonyStarkProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 325 + 115 + 170 + 65 + 80 + 40, MaterialUnit.Gram);

        tonyStarkProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        tonyStarkProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 6);
        tonyStarkProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 1);
        tonyStarkProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 2);

        await ConsumeMaterial(tonyStarkProject, DatabaseSeed.MaterialMagnet10x5, 8, MaterialUnit.Unit);
        await ConsumeMaterial(tonyStarkProject, DatabaseSeed.MaterialMagnet8x4, 8, MaterialUnit.Unit);
        await ConsumeMaterial(tonyStarkProject, DatabaseSeed.MaterialMagnet6x4, 4, MaterialUnit.Unit);

        await ConsumeMaterial(tonyStarkProject, DatabaseSeed.MaterialVallejoPrimer, 75 + 135, MaterialUnit.Drop);

        await ConsumeMaterial(tonyStarkProject, DatabaseSeed.MaterialArmyPainterAcrylic, 35 + 20 + 10 + 5 + 15 + 10 + 5 + 10 + 60 + 5 + 10 + 5 + 30 + 30 + 10 + 10, MaterialUnit.Drop);
        await ConsumeMaterial(tonyStarkProject, DatabaseSeed.MaterialTamiyaClear, 20, MaterialUnit.Mililiter);

        tonyStarkProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 1.5 + 1 + 1.5 + 0.5 + 2 + 1.5 + 1.5 + 3 + 2 + 3.5 + 2 + 3 + 2 + 2);

        context.Add(tonyStarkProject);

        context.SaveChanges();

        #endregion

        #region Kassandra

        var kassandraProject = new Project("rodrigo.basniak", "Kassandra", new DateTime(2025, 01, 01), modelId: null);

        kassandraProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (155 + 105 + 215 + 175) / 15);
        await ConsumeMaterial(kassandraProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 245 + 60 + 70 + 35 + 155 + 100 + 250, MaterialUnit.Gram);

        kassandraProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        kassandraProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 6);
        kassandraProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 1);
        kassandraProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 2);

        await ConsumeMaterial(kassandraProject, DatabaseSeed.MaterialMagnet10x5, 4, MaterialUnit.Unit);
        await ConsumeMaterial(kassandraProject, DatabaseSeed.MaterialMagnet8x4, 8, MaterialUnit.Unit);
        await ConsumeMaterial(kassandraProject, DatabaseSeed.MaterialMagnet6x4, 2, MaterialUnit.Unit);
        await ConsumeMaterial(kassandraProject, DatabaseSeed.MaterialMagnet3x2, 12, MaterialUnit.Unit);

        await ConsumeMaterial(kassandraProject, DatabaseSeed.MaterialVallejoPrimer, 23, MaterialUnit.Mililiter);

        await ConsumeMaterial(kassandraProject, DatabaseSeed.MaterialArmyPainterAcrylic, 15 + 40 + 10 + 10 + 5 + 70 + 40 + 2 + 30 + 5 + 30 + 15 + 10 + 310, MaterialUnit.Drop);

        kassandraProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 1.5 + 1 + 1.5 + 1.5 + 0.25 + 1.25 + 2 + 1.5 + 1 + 3 + 7 + 3 + 5 + 2);

        context.Add(kassandraProject);

        context.SaveChanges();

        #endregion

        #region Rogue vs Miss Marvel

        var missMarvelProject = new Project("rodrigo.basniak", "Rogue vs Miss Marvel", new DateTime(2025, 01, 01), modelId: null);

        missMarvelProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (160 + 90 + 200) / 15.0);
        await ConsumeMaterial(missMarvelProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 155 + 70 + 100 + 50 + 135 + 75, MaterialUnit.Gram);

        missMarvelProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        missMarvelProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 6);
        missMarvelProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 1);
        missMarvelProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 3);

        await ConsumeMaterial(missMarvelProject, DatabaseSeed.MaterialMagnet10x5, 8, MaterialUnit.Unit);
        await ConsumeMaterial(missMarvelProject, DatabaseSeed.MaterialMagnet8x4, 8, MaterialUnit.Unit);
        await ConsumeMaterial(missMarvelProject, DatabaseSeed.MaterialMagnet6x4, 6, MaterialUnit.Unit);
        await ConsumeMaterial(missMarvelProject, DatabaseSeed.MaterialMagnet3x2, 4, MaterialUnit.Unit);

        await ConsumeMaterial(missMarvelProject, DatabaseSeed.MaterialVallejoPrimer, 225, MaterialUnit.Drop);

        await ConsumeMaterial(missMarvelProject, DatabaseSeed.MaterialArmyPainterAcrylic, 60 + 10 + 15 + 15 + 30 + 20 + 10 + 10 + 5 + 5 + 5 + 45 + 20 + 20 + 45 + 20 + 20 + 20 + 5 + 50 + 100 + 35 + 10 + 40 + 10 + 40 + 20 + 20, MaterialUnit.Drop);

        await ConsumeMaterial(missMarvelProject, DatabaseSeed.MaterialVallejoVarnish, 150, MaterialUnit.Drop);

        missMarvelProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 1.5 + 0.25 + 0.25 + 2 + 1.5 + 2 + 4.5 + 5 + 12);

        context.Add(missMarvelProject);

        context.SaveChanges();

        #endregion

        #region Yennefer

        var yenneferProject = new Project("rodrigo.basniak", "Yennefer", new DateTime(2025, 01, 01), modelId: null);

        yenneferProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (85 + 140) / 15.0);
        await ConsumeMaterial(yenneferProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 95 + 20 + 210 + 35, MaterialUnit.Gram);

        yenneferProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        yenneferProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 4);
        yenneferProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 0.5);
        yenneferProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 1);

        await ConsumeMaterial(yenneferProject, DatabaseSeed.MaterialMagnet6x4, 12, MaterialUnit.Unit);

        await ConsumeMaterial(yenneferProject, DatabaseSeed.MaterialVallejoPrimer, 75, MaterialUnit.Drop);

        await ConsumeMaterial(yenneferProject, DatabaseSeed.MaterialArmyPainterAcrylic, 10 + 10 + 20 + 20 + 15 + 15 + 10 + 10 + 5 + 5 + 5 + 5 + 30 + 10 + 50 + 5 + 5 + 10 + 10 + 5 + 10, MaterialUnit.Drop);

        await ConsumeMaterial(yenneferProject, DatabaseSeed.MaterialVallejoVarnish, 75, MaterialUnit.Drop);

        yenneferProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 25);

        context.Add(yenneferProject);

        context.SaveChanges();

        #endregion

        #region Wonder Woman

        var wonderWomanProject = new Project("rodrigo.basniak", "Wonder Woman", new DateTime(2025, 01, 01), modelId: null);

        wonderWomanProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (145 + 155) / 15.0);
        await ConsumeMaterial(wonderWomanProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 130 + 60 + 150 + 45, MaterialUnit.Gram);

        wonderWomanProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        wonderWomanProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 4);
        wonderWomanProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 0.5);
        wonderWomanProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 1.5);

        await ConsumeMaterial(wonderWomanProject, DatabaseSeed.MaterialMagnet10x5, 6, MaterialUnit.Unit);
        await ConsumeMaterial(wonderWomanProject, DatabaseSeed.MaterialMagnet8x4, 12, MaterialUnit.Unit);
        await ConsumeMaterial(wonderWomanProject, DatabaseSeed.MaterialMagnet6x4, 6, MaterialUnit.Unit);
        await ConsumeMaterial(wonderWomanProject, DatabaseSeed.MaterialMagnet5x3, 4, MaterialUnit.Unit);

        await ConsumeMaterial(wonderWomanProject, DatabaseSeed.MaterialVallejoPrimer, 200 + 325, MaterialUnit.Drop);

        await ConsumeMaterial(wonderWomanProject, DatabaseSeed.MaterialArmyPainterAcrylic, 15 + 15 + 30 + 9 + 6 + 7 + 435, MaterialUnit.Drop);
        await ConsumeMaterial(wonderWomanProject, DatabaseSeed.MaterialVallejoAcrylic, 000000000, MaterialUnit.Drop);
        await ConsumeMaterial(wonderWomanProject, DatabaseSeed.MaterialAtomAcrylic, 000000000, MaterialUnit.Drop);

        await ConsumeMaterial(wonderWomanProject, DatabaseSeed.MaterialVallejoVarnish, 000000000, MaterialUnit.Drop);

        wonderWomanProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 25);

        context.Add(wonderWomanProject);

        context.SaveChanges();

        #endregion

        #region Black Widow

        var blackWidowsProject = new Project("rodrigo.basniak", "Black Widow", new DateTime(2025, 01, 01), modelId: null);

        blackWidowsProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (120) / 15.0);
        await ConsumeMaterial(blackWidowsProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 150 + 60, MaterialUnit.Gram);

        blackWidowsProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 0);
        blackWidowsProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 2);
        blackWidowsProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 0.25);
        blackWidowsProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 1);

        await ConsumeMaterial(blackWidowsProject, DatabaseSeed.MaterialMagnet8x4, 10, MaterialUnit.Unit);
        await ConsumeMaterial(blackWidowsProject, DatabaseSeed.MaterialMagnet6x4, 6, MaterialUnit.Unit);

        await ConsumeMaterial(blackWidowsProject, DatabaseSeed.MaterialVallejoPrimer, 150 + 15 + 10, MaterialUnit.Drop);

        await ConsumeMaterial(blackWidowsProject, DatabaseSeed.MaterialArmyPainterAcrylic, 15 + 5 + 5 + 5 + 30 + 30 + 10 + 50 + 10 + 30, MaterialUnit.Drop);

        await ConsumeMaterial(blackWidowsProject, DatabaseSeed.MaterialVallejoVarnish, 60, MaterialUnit.Drop);

        blackWidowsProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 0.5 + 1 + 1 + 1 + 4 + 9 + 2);

        context.Add(blackWidowsProject);

        context.SaveChanges();

        #endregion

        #region Lightning

        var lightningProject = new Project("rodrigo.basniak", "Lightning", new DateTime(2025, 01, 01), modelId: null);

        lightningProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (130 + 110) / 15.0);
        await ConsumeMaterial(lightningProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 100 + 40 + 80 + 30, MaterialUnit.Gram);

        lightningProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        lightningProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 4);
        lightningProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 0.5);
        lightningProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 2);

        await ConsumeMaterial(lightningProject, DatabaseSeed.MaterialMagnet10x5, 4, MaterialUnit.Unit);
        await ConsumeMaterial(lightningProject, DatabaseSeed.MaterialMagnet8x4, 4, MaterialUnit.Unit);
        await ConsumeMaterial(lightningProject, DatabaseSeed.MaterialMagnet6x4, 8, MaterialUnit.Unit);

        await ConsumeMaterial(lightningProject, DatabaseSeed.MaterialVallejoPrimer, 550, MaterialUnit.Drop);

        await ConsumeMaterial(lightningProject, DatabaseSeed.MaterialArmyPainterAcrylic, 240 + 50 + 75 + 15 + 70, MaterialUnit.Drop);

        await ConsumeMaterial(lightningProject, DatabaseSeed.MaterialVallejoVarnish, 100, MaterialUnit.Drop);

        lightningProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 20);

        context.Add(lightningProject);

        context.SaveChanges();

        #endregion

        #region Thrall

        var thrallProject = new Project("rodrigo.basniak", "Thrall", new DateTime(2025, 01, 01), modelId: null);

        thrallProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (130 + 90 + 125) / 15.0);
        await ConsumeMaterial(thrallProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 145 + 60 + 85 + 40 + 95 + 35, MaterialUnit.Gram);

        thrallProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        thrallProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 6);
        thrallProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 1);
        thrallProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 2.5);

        await ConsumeMaterial(thrallProject, DatabaseSeed.MaterialMagnet10x5, 16, MaterialUnit.Unit);
        await ConsumeMaterial(thrallProject, DatabaseSeed.MaterialMagnet8x4, 4, MaterialUnit.Unit);

        await ConsumeMaterial(thrallProject, DatabaseSeed.MaterialVallejoPrimer, 450, MaterialUnit.Drop);

        await ConsumeMaterial(thrallProject, DatabaseSeed.MaterialArmyPainterAcrylic, 30 + 15 + 30 + 10 + 10 + 15 + 5 + 25 + 15 + 25 + 10 + 25 + 20 + 40 + 30 + 10, MaterialUnit.Drop);

        await ConsumeMaterial(thrallProject, DatabaseSeed.MaterialVallejoVarnish, 200, MaterialUnit.Drop);

        thrallProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 1 + 1 + 3 + 2 + 2 + 6 + 4 + 2);

        context.Add(thrallProject);

        context.SaveChanges();

        #endregion

        #region Donkey Kong

        var DonkeyKongProject = new Project("rodrigo.basniak", "Donkey Kong", new DateTime(2025, 01, 01), modelId: null);

        DonkeyKongProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (85 + 175 + 100) / 15.0);
        await ConsumeMaterial(DonkeyKongProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 110 + 30 + 250 + 90 + 70 + 20, MaterialUnit.Gram);

        DonkeyKongProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        DonkeyKongProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 5);
        DonkeyKongProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 1);
        DonkeyKongProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 6);

        await ConsumeMaterial(DonkeyKongProject, DatabaseSeed.MaterialMagnet6x4, 20, MaterialUnit.Unit);
        await ConsumeMaterial(DonkeyKongProject, DatabaseSeed.MaterialMagnet8x4, 4, MaterialUnit.Unit);
        await ConsumeMaterial(DonkeyKongProject, DatabaseSeed.MaterialMagnet3x2, 4, MaterialUnit.Unit);

        await ConsumeMaterial(DonkeyKongProject, DatabaseSeed.MaterialVallejoPrimer, 450, MaterialUnit.Drop);

        await ConsumeMaterial(DonkeyKongProject, DatabaseSeed.MaterialArmyPainterAcrylic, 15 + 5 + 5 + 3 + 5 + 120 + 5 + 10 + 5 + 5 + 5 + 5 + 20 + 6 + 6 + 20 + 20 + 5 + 5 + 5 + 20 + 15 + 50 + 15 + 15 + 30 + 25 + 75, MaterialUnit.Drop);

        await ConsumeMaterial(DonkeyKongProject, DatabaseSeed.MaterialVallejoVarnish, 200, MaterialUnit.Drop);

        DonkeyKongProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 2.5 + 1 + 1 + 1.5 + 3 + 2 + 2 + 8 + 4);

        context.Add(DonkeyKongProject);

        context.SaveChanges();

        #endregion

        #region Final Fantasy Diorama

        var ffxivProject = new Project("rodrigo.basniak", "Final Fantasy Diorama", new DateTime(2025, 01, 01), modelId: null);

        ffxivProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (175 + 165 + 160 + 165 + 105 + 85) / 15.0);
        await ConsumeMaterial(ffxivProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 140 + 80 + 190 + 45 + 175 + 55 + 170 + 55 + 85 + 35 + 95 + 40, MaterialUnit.Gram);

        ffxivProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        ffxivProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 10);
        ffxivProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 1.5);
        ffxivProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 4);

        await ConsumeMaterial(ffxivProject, DatabaseSeed.MaterialMagnet10x5, 12, MaterialUnit.Unit);
        await ConsumeMaterial(ffxivProject, DatabaseSeed.MaterialMagnet8x4, 16, MaterialUnit.Unit);
        await ConsumeMaterial(ffxivProject, DatabaseSeed.MaterialMagnet6x4, 15, MaterialUnit.Unit);

        await ConsumeMaterial(ffxivProject, DatabaseSeed.MaterialVallejoPrimer, 400, MaterialUnit.Drop);

        await ConsumeMaterial(ffxivProject, DatabaseSeed.MaterialArmyPainterAcrylic, 720, MaterialUnit.Drop);

        await ConsumeMaterial(ffxivProject, DatabaseSeed.MaterialVallejoVarnish, 300, MaterialUnit.Drop);

        ffxivProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 34);

        context.Add(ffxivProject);

        context.SaveChanges();

        #endregion

        #region Yunna

        var yunnaProject = new Project("rodrigo.basniak", "Yunna", new DateTime(2025, 01, 01), modelId: null);

        yunnaProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (100 + 150) / 15.0);
        await ConsumeMaterial(yunnaProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 75 + 45 + 75 + 35, MaterialUnit.Gram);

        yunnaProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        yunnaProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 4);
        yunnaProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 0.5);
        yunnaProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 2);

        await ConsumeMaterial(yunnaProject, DatabaseSeed.MaterialMagnet10x5, 6, MaterialUnit.Unit);
        await ConsumeMaterial(yunnaProject, DatabaseSeed.MaterialMagnet8x4, 4, MaterialUnit.Unit);
        await ConsumeMaterial(yunnaProject, DatabaseSeed.MaterialMagnet6x4, 6, MaterialUnit.Unit);

        await ConsumeMaterial(yunnaProject, DatabaseSeed.MaterialVallejoPrimer, 300, MaterialUnit.Drop);

        await ConsumeMaterial(yunnaProject, DatabaseSeed.MaterialArmyPainterAcrylic, 250, MaterialUnit.Drop);

        await ConsumeMaterial(yunnaProject, DatabaseSeed.MaterialVallejoVarnish, 150, MaterialUnit.Drop);

        yunnaProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 23);

        context.Add(yunnaProject);

        context.SaveChanges();

        #endregion

        #region Makima

        var makimaProject = new Project("rodrigo.basniak", "Makima", new DateTime(2025, 01, 01), modelId: null);

        makimaProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (160) / 17);
        await ConsumeMaterial(makimaProject, DatabaseSeed.MaterialJayoAbsResin, 100 + 60, MaterialUnit.Gram);

        makimaProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 0.5);
        makimaProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 2);
        makimaProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 0.5);
        makimaProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 1);

        await ConsumeMaterial(makimaProject, DatabaseSeed.MaterialMagnet10x5, 4, MaterialUnit.Unit);
        await ConsumeMaterial(makimaProject, DatabaseSeed.MaterialMagnet8x4, 2, MaterialUnit.Unit);

        await ConsumeMaterial(makimaProject, DatabaseSeed.MaterialVallejoPrimer, 000000000, MaterialUnit.Drop);

        await ConsumeMaterial(makimaProject, DatabaseSeed.MaterialArmyPainterAcrylic, 000000000, MaterialUnit.Drop);

        await ConsumeMaterial(makimaProject, DatabaseSeed.MaterialVallejoVarnish, 000000000, MaterialUnit.Drop);

        makimaProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 000000000);

        context.Add(makimaProject);

        context.SaveChanges();

        #endregion

        #region DemoProject

        var referenceDate = new DateTime(2026, 01, 01);

        var demoProject = new Project("rodrigo.basniak", "DemoProject", referenceDate, modelId: null);

        // Two records for each project step
        demoProject.AddExecutionWindow(ProjectStepDefinition.Planning, referenceDate, 1.5);
        demoProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2026, 01, 24), 1.0);
        demoProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2026, 01, 25), (160 + 145) / 15.0);
        demoProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2026, 01, 26), (130 + 120) / 15.0);
        demoProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2026, 01, 27), 3.0);
        demoProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2026, 01, 28), 2.0);
        demoProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2026, 01, 27), 1.0);
        demoProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2026, 01, 28), 0.75);
        demoProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2026, 01, 28), 2.25);
        demoProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2026, 01, 29), 1.5);
        demoProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2026, 01, 30), 6.0);
        demoProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2026, 02, 02), 5.5);

        // Materials: 2-3 from each category
        await ConsumeMaterial(demoProject, DatabaseSeed.MaterialSunluWaterWashableStandardResin, 420, MaterialUnit.Gram);
        await ConsumeMaterial(demoProject, DatabaseSeed.MaterialJayoAbsResin, 210, MaterialUnit.Gram);

        await ConsumeMaterial(demoProject, DatabaseSeed.MaterialMagnet10x5, 8, MaterialUnit.Unit);
        await ConsumeMaterial(demoProject, DatabaseSeed.MaterialMagnet8x4, 10, MaterialUnit.Unit);
        await ConsumeMaterial(demoProject, DatabaseSeed.MaterialMagnet6x4, 6, MaterialUnit.Unit);

        await ConsumeMaterial(demoProject, DatabaseSeed.MaterialVallejoPrimer, 160, MaterialUnit.Drop);
        await ConsumeMaterial(demoProject, DatabaseSeed.MaterialGswPrimer, 120, MaterialUnit.Drop);

        await ConsumeMaterial(demoProject, DatabaseSeed.MaterialArmyPainterAcrylic, 260, MaterialUnit.Drop);
        await ConsumeMaterial(demoProject, DatabaseSeed.MaterialVallejoAcrylic, 120, MaterialUnit.Drop);
        await ConsumeMaterial(demoProject, DatabaseSeed.MaterialAtomAcrylic, 75, MaterialUnit.Drop);

        await ConsumeMaterial(demoProject, DatabaseSeed.MaterialVallejoVarnish, 90, MaterialUnit.Drop);
        await ConsumeMaterial(demoProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 35, MaterialUnit.Spray);

        await ConsumeMaterial(demoProject, DatabaseSeed.MaterialMaskingTapeGeneric10mm, 120, MaterialUnit.Centimeter);
        await ConsumeMaterial(demoProject, DatabaseSeed.MaterialMaskingTapeTamiya10mm, 90, MaterialUnit.Centimeter);
        await ConsumeMaterial(demoProject, DatabaseSeed.MaterialVallejoLiquidMask, 12, MaterialUnit.Mililiter);

        await ConsumeMaterial(demoProject, DatabaseSeed.MaterialDisposableBrush, 4, MaterialUnit.Unit);
        await ConsumeMaterial(demoProject, DatabaseSeed.MaterialPaintMixingCupCorrugated, 6, MaterialUnit.Unit);
        await ConsumeMaterial(demoProject, DatabaseSeed.MaterialNailWoodSwab, 30, MaterialUnit.Unit);

        context.Add(demoProject);

        // Finished pictures (>= 3)
        var sampleFinishedPictures = new[]
        {
            "uploads\\sample\\last_of_us\\project_01.png",
            "uploads\\sample\\last_of_us\\project_02.png",
            "uploads\\sample\\last_of_us\\project_03.png"
        };
        foreach (var x in sampleFinishedPictures)
        {
            demoProject.AddFinishedPicture(x);
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
            demoProject.AddReferencePicture(x);
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
            demoProject,
            "Skin",
            (ColorZone.Midtone, "#C29681", true),
            (ColorZone.Highlight, "#E0B59D", true),
            (ColorZone.Shadow, "#8C5F4F", true));

        AddColorGroup(
            demoProject,
            "Jacket Leather",
            (ColorZone.Midtone, "#6D4A33", true),
            (ColorZone.Highlight, "#9A6E4D", true),
            (ColorZone.Shadow, "#3A2A20", true));

        AddColorGroup(
            demoProject,
            "Jeans Denim",
            (ColorZone.Midtone, "#3D5E7A", true),
            (ColorZone.Highlight, "#6D8FAE", true),
            (ColorZone.Shadow, "#24384A", true));

        AddColorGroup(
            demoProject,
            "Backpack Canvas",
            (ColorZone.Midtone, "#5B6E4A", true),
            (ColorZone.Shadow, "#35422A", true));

        AddColorGroup(
            demoProject,
            "Metal Details",
            (ColorZone.Midtone, "#7A7F87", true),
            (ColorZone.Highlight, "#B6BCC4", true));

        AddColorGroup(
            demoProject,
            "Base Vegetation",
            (ColorZone.Midtone, "#4E603A", false));

        context.SaveChanges();

        #endregion

        #region Last of Us Diorama 

        referenceDate = referenceDate.AddDays(1);

        var lastOfUsProject = new Project("rodrigo.basniak", "Last Of Us Diorama", referenceDate, modelId: null);

        lastOfUsProject.AddExecutionWindow(ProjectStepDefinition.Printing, referenceDate, (175 + 95 + 130 + 175 + 135 + 205 + 145) / 15.0);
        await ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialJayoAbsResin, (220 + 60 + 90 + 45 + 85 + 55 + 170 + 35 + 155 + 70 + 210 + 75 + 115 + 75), MaterialUnit.Gram);

        lastOfUsProject.AddExecutionWindow(ProjectStepDefinition.Planning, referenceDate, 3);
        lastOfUsProject.AddExecutionWindow(ProjectStepDefinition.Supporting, referenceDate, 9);
        lastOfUsProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, referenceDate, 3);
        lastOfUsProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, referenceDate, 2.5 + 1 + 1 + 1);

        await ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialPaintMixingCupCorrugated, 4, MaterialUnit.Unit);
        await ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialDisposableBrush, 2, MaterialUnit.Unit);
        await ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialDspiaeSandingDisk, 2, MaterialUnit.Unit);
        await ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialNailWoodSwab, 35, MaterialUnit.Unit);
        await ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialGlooves, 6, MaterialUnit.Unit);

        await ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialMagnet10x5, 14, MaterialUnit.Unit);
        await ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialMagnet8x4, 8, MaterialUnit.Unit);
        await ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialMagnet6x4, 6, MaterialUnit.Unit);
        await ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialMagnet4x3, 4, MaterialUnit.Unit);

        await ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialVallejoPrimer, 275, MaterialUnit.Drop);

        await ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialArmyPainterAcrylic, 25 + 5 + 10 + 25 + 20 + 25 + 120 + 35 + 5 + 25 + 15 + 15 + 25 + 5 + 30 + 5 + 15, MaterialUnit.Drop);
        await ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialVallejoAcrylic, 70, MaterialUnit.Drop);
        await ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialAtomAcrylic, 10, MaterialUnit.Drop);
        await ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialAmmoAcrylic, 25, MaterialUnit.Drop);

        await ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialGswVarnish, 15, MaterialUnit.Drop);
        await ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialAmmoTransparator, 20 + 10 + 10 + 15 + 20 + 30 + 10 + 20 + 10 + 20, MaterialUnit.Drop);
        await ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialVallejoLiquidMask, 20, MaterialUnit.Drop);
        await ConsumeMaterial(lastOfUsProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 115, MaterialUnit.Spray);

        lastOfUsProject.AddExecutionWindow(ProjectStepDefinition.Painting, referenceDate, 1 + 2 + 5 + 3 + 0.5 + 2 + 0.5 + 2.5 + 7 + 6 + 5 + 1);

        context.Add(lastOfUsProject);

        var sampleDirectory = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "PaintingProjectsManagement.Api", "wwwroot", "uploads", "sample", "LastOfUs");
        if (Directory.Exists(sampleDirectory))
        {
            var imageFiles = Directory.GetFiles(sampleDirectory)
                .OrderBy(x => x)
                .ToList();

            foreach (var imageFile in imageFiles)
            {
                var fileName = Path.GetFileName(imageFile);
                lastOfUsProject.AddReferencePicture($"uploads\\sample\\LastOfUs\\{fileName}");
            }
        }

        context.SaveChanges();

        #endregion

        #region Witch King Diorama 

        referenceDate = referenceDate.AddDays(1);

        var witchKingDiorama = new Project("rodrigo.basniak", "Witch King Diorama", referenceDate, modelId: null);

        witchKingDiorama.AddExecutionWindow(ProjectStepDefinition.Printing, referenceDate, (150 + 175 + 140 + 195 + 190 + 215 + 220 + 200 + 165 + 180 + 180 + 120) / 15.0);
        await ConsumeMaterial(witchKingDiorama, DatabaseSeed.MaterialJayoAbsResin, (225 + 145 + 205 + 150 + 145 + 100 + 215 + 100 + 190 + 140 + 180 + 140 + 100 + 125 + 160 + 85 + 90 + 115 + 90 + 90 + 50 + 100 + 70 + 35), MaterialUnit.Gram);

        witchKingDiorama.AddExecutionWindow(ProjectStepDefinition.Planning, referenceDate, 2);
        witchKingDiorama.AddExecutionWindow(ProjectStepDefinition.Supporting, referenceDate, 13.5);
        witchKingDiorama.AddExecutionWindow(ProjectStepDefinition.Cleaning, referenceDate, 4);
        witchKingDiorama.AddExecutionWindow(ProjectStepDefinition.PostProcessing, referenceDate, 6.5);

        await ConsumeMaterial(witchKingDiorama, DatabaseSeed.MaterialDisposableBrush, 2, MaterialUnit.Unit);
        await ConsumeMaterial(witchKingDiorama, DatabaseSeed.MaterialDspiaeSandingDisk, 4, MaterialUnit.Unit);
        await ConsumeMaterial(witchKingDiorama, DatabaseSeed.MaterialNailWoodSwab, 15, MaterialUnit.Unit);
        await ConsumeMaterial(witchKingDiorama, DatabaseSeed.MaterialGlooves, 12, MaterialUnit.Unit);

        await ConsumeMaterial(witchKingDiorama, DatabaseSeed.MaterialMagnet10x5, 32, MaterialUnit.Unit);
        await ConsumeMaterial(witchKingDiorama, DatabaseSeed.MaterialMagnet8x4, 14, MaterialUnit.Unit);

        await ConsumeMaterial(witchKingDiorama, DatabaseSeed.MaterialVallejoPrimer, 25 + 75 + 100 + 20 + 50 + 100 + 15 + 150, MaterialUnit.Drop);
        await ConsumeMaterial(witchKingDiorama, DatabaseSeed.MaterialGswPrimer, 120 + 100 + 30, MaterialUnit.Drop);

        await ConsumeMaterial(witchKingDiorama, DatabaseSeed.MaterialArmyPainterAcrylic, 20 + 20 + 15 + 15 + 20 + 200 + 75 + 10 + 30 + 35 + 40 + 30 + 15, MaterialUnit.Drop);
        await ConsumeMaterial(witchKingDiorama, DatabaseSeed.MaterialVallejoAcrylic, 30 + 5 + 10 + 5, MaterialUnit.Drop);
        await ConsumeMaterial(witchKingDiorama, DatabaseSeed.MaterialAmmoDrybrush, 25, MaterialUnit.Drop);
        await ConsumeMaterial(witchKingDiorama, DatabaseSeed.MaterialAmmoAcrylic, 5, MaterialUnit.Drop);
        await ConsumeMaterial(witchKingDiorama, DatabaseSeed.MaterialGswAcrylic, 20 + 10 + 25 + 15, MaterialUnit.Drop);

        await ConsumeMaterial(witchKingDiorama, DatabaseSeed.MaterialVallejoVarnish, 100 + 25, MaterialUnit.Drop);
        await ConsumeMaterial(witchKingDiorama, DatabaseSeed.MaterialAmmoTransparator, 200, MaterialUnit.Drop);
        await ConsumeMaterial(witchKingDiorama, DatabaseSeed.MaterialBiltemaMatteVarnish, 110, MaterialUnit.Spray);

        witchKingDiorama.AddExecutionWindow(ProjectStepDefinition.Painting, referenceDate, 4 + 5.5 + 5 + 1.5 + 4.5 + 3 + 3 + 3.5 + 6 + 2);

        context.Add(witchKingDiorama);

        #endregion

        #region Jubileu 

        referenceDate = referenceDate.AddDays(1);

        var jubileeProject = new Project("rodrigo.basniak", "Jubileu", referenceDate, modelId: null);

        jubileeProject.AddExecutionWindow(ProjectStepDefinition.Printing, referenceDate, (250) / 15.0);
        await ConsumeMaterial(jubileeProject, DatabaseSeed.MaterialJayoAbsResin, (120 + 70 + 55 + 30), MaterialUnit.Gram);

        jubileeProject.AddExecutionWindow(ProjectStepDefinition.Planning, referenceDate, 1);
        jubileeProject.AddExecutionWindow(ProjectStepDefinition.Supporting, referenceDate, 3);
        jubileeProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, referenceDate, 0.5);
        jubileeProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, referenceDate, 1);

        await ConsumeMaterial(jubileeProject, DatabaseSeed.MaterialPaintMixingCupCorrugated, 5, MaterialUnit.Unit);
        await ConsumeMaterial(jubileeProject, DatabaseSeed.MaterialDisposableBrush, 5, MaterialUnit.Unit);
        await ConsumeMaterial(jubileeProject, DatabaseSeed.MaterialDspiaeSandingDisk, 2, MaterialUnit.Unit);
        await ConsumeMaterial(jubileeProject, DatabaseSeed.MaterialNailWoodSwab, 50, MaterialUnit.Unit);
        await ConsumeMaterial(jubileeProject, DatabaseSeed.MaterialGlooves, 5, MaterialUnit.Unit);
        await ConsumeMaterial(jubileeProject, DatabaseSeed.MaterialMaskingTapeTamiyaFlexible2mm, 30, MaterialUnit.Centimeter);
        await ConsumeMaterial(jubileeProject, DatabaseSeed.MaterialMaskingTapeGeneric25mm, 100, MaterialUnit.Centimeter);

        await ConsumeMaterial(jubileeProject, DatabaseSeed.MaterialMagnet10x5, 4, MaterialUnit.Unit);
        await ConsumeMaterial(jubileeProject, DatabaseSeed.MaterialMagnet8x4, 4, MaterialUnit.Unit);
        await ConsumeMaterial(jubileeProject, DatabaseSeed.MaterialMagnet6x4, 6, MaterialUnit.Unit);

        await ConsumeMaterial(jubileeProject, DatabaseSeed.MaterialVallejoPrimer, 40, MaterialUnit.Drop);

        await ConsumeMaterial(jubileeProject, DatabaseSeed.MaterialArmyPainterAcrylic, 25 + 50 + 90 + 40, MaterialUnit.Drop);
        await ConsumeMaterial(jubileeProject, DatabaseSeed.MaterialVallejoAcrylic, 30, MaterialUnit.Drop);
        await ConsumeMaterial(jubileeProject, DatabaseSeed.MaterialAmmoAcrylic, 25 + 35 + 15, MaterialUnit.Drop);
        await ConsumeMaterial(jubileeProject, DatabaseSeed.MaterialGswAcrylic, 5, MaterialUnit.Drop);

        await ConsumeMaterial(jubileeProject, DatabaseSeed.MaterialGswVarnish, 25, MaterialUnit.Drop);
        await ConsumeMaterial(jubileeProject, DatabaseSeed.MaterialVallejoVarnish, 35, MaterialUnit.Drop);
        await ConsumeMaterial(jubileeProject, DatabaseSeed.MaterialAmmoTransparator, 50, MaterialUnit.Drop);
        await ConsumeMaterial(jubileeProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 20 + 30 + 25 + 10, MaterialUnit.Spray);

        jubileeProject.AddExecutionWindow(ProjectStepDefinition.Painting, referenceDate, 1.5 + 0.5 + 6 + 2.5 + 5 + 1 + .5);

        context.Add(jubileeProject);

        #endregion

        #region Makima  

        referenceDate = referenceDate.AddDays(1);

        var makimaCaStudiosProject = new Project("rodrigo.basniak", "Makima", referenceDate, modelId: null);

        makimaCaStudiosProject.AddExecutionWindow(ProjectStepDefinition.Printing, referenceDate, (155 + 120 + 190) / 15.0);
        await ConsumeMaterial(makimaCaStudiosProject, DatabaseSeed.MaterialJayoAbsResin, 120 + 60, MaterialUnit.Gram);
        await ConsumeMaterial(makimaCaStudiosProject, DatabaseSeed.MaterialAnycubricClearResin, 135 + 70 + 210 + 65, MaterialUnit.Gram);

        makimaCaStudiosProject.AddExecutionWindow(ProjectStepDefinition.Planning, referenceDate, 2);
        makimaCaStudiosProject.AddExecutionWindow(ProjectStepDefinition.Supporting, referenceDate, 3);
        makimaCaStudiosProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, referenceDate, 1);
        makimaCaStudiosProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, referenceDate, 2.5);

        await ConsumeMaterial(makimaCaStudiosProject, DatabaseSeed.MaterialPaintMixingCupCorrugated, 2, MaterialUnit.Unit);
        await ConsumeMaterial(makimaCaStudiosProject, DatabaseSeed.MaterialDisposableBrush, 1, MaterialUnit.Unit);
        await ConsumeMaterial(makimaCaStudiosProject, DatabaseSeed.MaterialDspiaeSandingDisk, 2, MaterialUnit.Unit);
        await ConsumeMaterial(makimaCaStudiosProject, DatabaseSeed.MaterialNailWoodSwab, 5, MaterialUnit.Unit);
        await ConsumeMaterial(makimaCaStudiosProject, DatabaseSeed.MaterialGlooves, 4, MaterialUnit.Unit);

        await ConsumeMaterial(makimaCaStudiosProject, DatabaseSeed.MaterialMagnet10x5, 6, MaterialUnit.Unit);
        await ConsumeMaterial(makimaCaStudiosProject, DatabaseSeed.MaterialMagnet8x4, 8, MaterialUnit.Unit);
        await ConsumeMaterial(makimaCaStudiosProject, DatabaseSeed.MaterialMagnet6x4, 2, MaterialUnit.Unit);
        await ConsumeMaterial(makimaCaStudiosProject, DatabaseSeed.MaterialMagnet4x3, 4, MaterialUnit.Unit);

        await ConsumeMaterial(makimaCaStudiosProject, DatabaseSeed.MaterialVallejoPrimer, 110, MaterialUnit.Drop);

        await ConsumeMaterial(makimaCaStudiosProject, DatabaseSeed.MaterialArmyPainterAcrylic, 15 + 15 + 30 + 50 + 10 + 10 + 85 + 30, MaterialUnit.Drop);
        await ConsumeMaterial(makimaCaStudiosProject, DatabaseSeed.MaterialAtomAcrylic, 10, MaterialUnit.Drop);
        await ConsumeMaterial(makimaCaStudiosProject, DatabaseSeed.MaterialAmmoCandy, 5, MaterialUnit.Mililiter);
        await ConsumeMaterial(makimaCaStudiosProject, DatabaseSeed.MaterialTamiyaClear, 9 * 50, MaterialUnit.Drop);

        await ConsumeMaterial(makimaCaStudiosProject, DatabaseSeed.MaterialVallejoVarnish, 5, MaterialUnit.Drop);
        await ConsumeMaterial(makimaCaStudiosProject, DatabaseSeed.MaterialGswVarnish, 50, MaterialUnit.Drop);
        await ConsumeMaterial(makimaCaStudiosProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 60, MaterialUnit.Spray);

        makimaCaStudiosProject.AddExecutionWindow(ProjectStepDefinition.Painting, referenceDate, 1 + 3.5 + 0.5 + 0.5 + 2 + 1.5 + 1 + 1.5);

        context.Add(makimaCaStudiosProject);

        #endregion

        #region Supergirl 

        referenceDate = referenceDate.AddDays(1);

        var supergirlProject = new Project("rodrigo.basniak", "Supergirl", referenceDate, modelId: null);

        supergirlProject.AddExecutionWindow(ProjectStepDefinition.Printing, referenceDate, (145 + 100) / 15.0);
        await ConsumeMaterial(supergirlProject, DatabaseSeed.MaterialJayoAbsResin, (90 + 50 + 55 + 30), MaterialUnit.Gram);

        supergirlProject.AddExecutionWindow(ProjectStepDefinition.Planning, referenceDate, 1);
        supergirlProject.AddExecutionWindow(ProjectStepDefinition.Supporting, referenceDate, 2.5);
        supergirlProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, referenceDate, 0.5);
        supergirlProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, referenceDate, 1.5);

        await ConsumeMaterial(supergirlProject, DatabaseSeed.MaterialDisposableBrush, 1, MaterialUnit.Unit);
        await ConsumeMaterial(supergirlProject, DatabaseSeed.MaterialDspiaeSandingDisk, 2, MaterialUnit.Unit);
        await ConsumeMaterial(supergirlProject, DatabaseSeed.MaterialNailWoodSwab, 20, MaterialUnit.Unit);
        await ConsumeMaterial(supergirlProject, DatabaseSeed.MaterialGlooves, 4, MaterialUnit.Unit);

        await ConsumeMaterial(supergirlProject, DatabaseSeed.MaterialMagnet10x5, 4, MaterialUnit.Unit);
        await ConsumeMaterial(supergirlProject, DatabaseSeed.MaterialMagnet6x4, 4, MaterialUnit.Unit);

        await ConsumeMaterial(supergirlProject, DatabaseSeed.MaterialVallejoPrimer, 250, MaterialUnit.Drop);

        await ConsumeMaterial(supergirlProject, DatabaseSeed.MaterialArmyPainterAcrylic, 30 + 45 + 30 + 15 + 10, MaterialUnit.Drop);
        await ConsumeMaterial(supergirlProject, DatabaseSeed.MaterialVallejoAcrylic, 20, MaterialUnit.Drop);
        await ConsumeMaterial(supergirlProject, DatabaseSeed.MaterialAmmoAcrylic, 15, MaterialUnit.Drop);

        await ConsumeMaterial(supergirlProject, DatabaseSeed.MaterialVallejoVarnish, 20, MaterialUnit.Drop);
        await ConsumeMaterial(supergirlProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 80, MaterialUnit.Spray);

        supergirlProject.AddExecutionWindow(ProjectStepDefinition.Painting, referenceDate, 0.5 + 0.5 + 0.5 + 2 + 7 + 1.5);

        context.Add(supergirlProject);

        #endregion

        #region Harley Quinn 

        referenceDate = referenceDate.AddDays(1);

        var harleyQuinnProject = new Project("rodrigo.basniak", "Harley Quinn", referenceDate, modelId: null);

        harleyQuinnProject.AddExecutionWindow(ProjectStepDefinition.Printing, referenceDate, (105 + 180) / 15.0);
        await ConsumeMaterial(harleyQuinnProject, DatabaseSeed.MaterialJayoAbsResin, (225 + 105), MaterialUnit.Gram);

        harleyQuinnProject.AddExecutionWindow(ProjectStepDefinition.Planning, referenceDate, 0.5);
        harleyQuinnProject.AddExecutionWindow(ProjectStepDefinition.Supporting, referenceDate, 2);
        harleyQuinnProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, referenceDate, 0.5);
        harleyQuinnProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, referenceDate, 1);

        await ConsumeMaterial(harleyQuinnProject, DatabaseSeed.MaterialDisposableBrush, 1, MaterialUnit.Unit);
        await ConsumeMaterial(harleyQuinnProject, DatabaseSeed.MaterialDspiaeSandingDisk, 1, MaterialUnit.Unit);
        await ConsumeMaterial(harleyQuinnProject, DatabaseSeed.MaterialNailWoodSwab, 10, MaterialUnit.Unit);
        await ConsumeMaterial(harleyQuinnProject, DatabaseSeed.MaterialGlooves, 8, MaterialUnit.Unit);
        await ConsumeMaterial(harleyQuinnProject, DatabaseSeed.MaterialMaskingTapeGeneric10mm, 60, MaterialUnit.Centimeter);
        await ConsumeMaterial(harleyQuinnProject, DatabaseSeed.MaterialMaskingTapeGeneric25mm, 30 + 30, MaterialUnit.Centimeter);

        await ConsumeMaterial(harleyQuinnProject, DatabaseSeed.MaterialMagnet8x4, 4, MaterialUnit.Unit);
        await ConsumeMaterial(harleyQuinnProject, DatabaseSeed.MaterialMagnet6x4, 4, MaterialUnit.Unit);

        await ConsumeMaterial(harleyQuinnProject, DatabaseSeed.MaterialVallejoPrimer, 30, MaterialUnit.Drop);

        await ConsumeMaterial(harleyQuinnProject, DatabaseSeed.MaterialArmyPainterAcrylic, 20 + 10 + 20 + 50 + 5 + 10 + 5 + 5 + 10 + 5 + 5 + 5, MaterialUnit.Drop);
        await ConsumeMaterial(harleyQuinnProject, DatabaseSeed.MaterialVallejoAcrylic, 5 + 5 + 25 + 5 + 5 + 30, MaterialUnit.Drop);
        await ConsumeMaterial(harleyQuinnProject, DatabaseSeed.MaterialAtomAcrylic, 15 + 5 + 25 + 5 + 25, MaterialUnit.Drop);
        await ConsumeMaterial(harleyQuinnProject, DatabaseSeed.MaterialGswAcrylic, 10 + 10 + 15, MaterialUnit.Drop);
        await ConsumeMaterial(harleyQuinnProject, DatabaseSeed.MaterialAmmoTransparator, 20, MaterialUnit.Drop);

        await ConsumeMaterial(harleyQuinnProject, DatabaseSeed.MaterialGswVarnish, 30, MaterialUnit.Drop);
        await ConsumeMaterial(harleyQuinnProject, DatabaseSeed.MaterialVallejoLiquidMask, 45, MaterialUnit.Drop);
        await ConsumeMaterial(harleyQuinnProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 10 + 20 + 15 + 16, MaterialUnit.Spray);
        await ConsumeMaterial(harleyQuinnProject, DatabaseSeed.MaterialTamiyaTS80, 50, MaterialUnit.Spray);

        harleyQuinnProject.AddExecutionWindow(ProjectStepDefinition.Painting, referenceDate, 2 + 1 + 3 + 6 + 0.5 + 4 + 2 + 1.5 + 2 + 1.5 + .5 + 5 + 1 + 3);

        context.Add(harleyQuinnProject);

        #endregion

        #region Supernatural diorama 

        referenceDate = referenceDate.AddDays(1);

        var supernaturalProject = new Project("rodrigo.basniak", "Supernatural Diorama", referenceDate, modelId: null);

        supernaturalProject.AddExecutionWindow(ProjectStepDefinition.Printing, referenceDate, (135 + 195 + 200 + 190 + 205 + 220 + 220 + 200 + 135 + 215 + 195 + 105 + 210) / 15.0);
        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialJayoAbsResin, (75 + 70 + 145 + 115 + 150 + 95 + 135 + 115 + 135 + 110 + 180 + 120 + 225 + 135 + 75 + 70 + 85 + 65 + 170 + 55 + 160 + 60 + 40 + 15 + 205 + 65), MaterialUnit.Gram);

        supernaturalProject.AddExecutionWindow(ProjectStepDefinition.Planning, referenceDate, 5 + 10 + 10);
        supernaturalProject.AddExecutionWindow(ProjectStepDefinition.Supporting, referenceDate, 12);
        supernaturalProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, referenceDate, 5);
        supernaturalProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, referenceDate, 8 + 6 + 4 + 1.5 + 1 + 0.5 + 1.5);

        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialPaintMixingCupNonCorrugated, 16, MaterialUnit.Unit);
        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialDisposableBrush, 4, MaterialUnit.Unit);
        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialDspiaeSandingDisk, 4, MaterialUnit.Unit);
        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialNailWoodSwab, 100, MaterialUnit.Unit);
        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialGlooves, 10, MaterialUnit.Unit);

        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialMagnet10x5, 22, MaterialUnit.Unit);
        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialMagnet8x4, 12, MaterialUnit.Unit);
        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialMagnet6x4, 4, MaterialUnit.Unit);
        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialMagnet5x3, 2, MaterialUnit.Unit);

        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialVallejoPrimer, 75 + 50 + 5 + 60 + 150 + 75 + 25, MaterialUnit.Drop);
        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialGswPrimer, 20 + 300 + 25 + 125 + 200 + 25 + 15, MaterialUnit.Drop);

        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialArmyPainterAcrylic, 25 + 25 + 10 + 10 + 30 + 25 + 10 + 150 + 10 + 75 + 25 + 75 + 50 + 20 + 10 + 5 + 50 + 40 + 15 + 20 + 15 + 30 + 30 + 25, MaterialUnit.Drop);
        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialAmmoDrybrush, 20, MaterialUnit.Drop);
        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialVallejoAcrylic, 40 + 15 + 5 + 10 + 35 + 15 + 15 + 15 + 10 + 15 + 40, MaterialUnit.Drop);
        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialAtomAcrylic, 15, MaterialUnit.Drop);
        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialAmmoTransparator, 50 + 50 + 25 + 35 + 75 + 25 + 20 + 40 + 20 + 50 + 20 + 150 + 25, MaterialUnit.Drop);
        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialAmmoAcrylic, 10, MaterialUnit.Drop);
        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialGswAcrylic, 15, MaterialUnit.Drop);
        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialAkChrome, 75, MaterialUnit.Drop);
        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialGswChrome, 225, MaterialUnit.Drop);

        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialVallejoVarnish, 150 + 30 + 20 + 25 + 50, MaterialUnit.Drop);
        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialTamiyaTS80, 90, MaterialUnit.Spray);
        await ConsumeMaterial(supernaturalProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 15 + 60 + 21 + 25 + 16 + 40 + 42 + 30 + 45 + 12 + 27, MaterialUnit.Spray);

        supernaturalProject.AddExecutionWindow(ProjectStepDefinition.Painting, referenceDate, 3.5 + 4 + 0.5 + 2 + 6 + 4 + 1 + 1 + 1.5 + 0.5 + 0.5 + 5 + 2 + 4 + 4 + 1 + 1 + 2.5 + 3 + 3.5 + 3.5 + 2.5 + 8 + 1 + 5 + 2 + 4 + 2);

        context.Add(supernaturalProject);

        #endregion

        #region Dean and Sam Winchester

        referenceDate = referenceDate.AddDays(1);

        var danSamProject = new Project("rodrigo.basniak", "Dean and Sam Winchester", referenceDate, modelId: null);

        danSamProject.AddExecutionWindow(ProjectStepDefinition.Printing, referenceDate, (190 + 210 + 120 + 190) / 15.0);
        await ConsumeMaterial(danSamProject, DatabaseSeed.MaterialJayoAbsResin, (155 + 55 + 145 + 60 + 50 + 20 + 315 + 100), MaterialUnit.Gram);

        danSamProject.AddExecutionWindow(ProjectStepDefinition.Planning, referenceDate, 4);
        danSamProject.AddExecutionWindow(ProjectStepDefinition.Supporting, referenceDate, 3);
        danSamProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, referenceDate, 1);
        danSamProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, referenceDate, 2 + 2 + 1 + 0.5 + 1);

        await ConsumeMaterial(danSamProject, DatabaseSeed.MaterialDisposableBrush, 2, MaterialUnit.Unit);
        await ConsumeMaterial(danSamProject, DatabaseSeed.MaterialDspiaeSandingDisk, 2, MaterialUnit.Unit);
        await ConsumeMaterial(danSamProject, DatabaseSeed.MaterialNailWoodSwab, 25, MaterialUnit.Unit);
        await ConsumeMaterial(danSamProject, DatabaseSeed.MaterialGlooves, 4, MaterialUnit.Unit);

        await ConsumeMaterial(danSamProject, DatabaseSeed.MaterialMagnet10x5, 14, MaterialUnit.Unit);
        await ConsumeMaterial(danSamProject, DatabaseSeed.MaterialMagnet8x4, 16, MaterialUnit.Unit);
        await ConsumeMaterial(danSamProject, DatabaseSeed.MaterialMagnet6x4, 4, MaterialUnit.Unit);

        await ConsumeMaterial(danSamProject, DatabaseSeed.MaterialVallejoPrimer, 50 + 50 + 50 + 50 + 25 + 15 + 50, MaterialUnit.Drop);

        await ConsumeMaterial(danSamProject, DatabaseSeed.MaterialArmyPainterAcrylic, 200, MaterialUnit.Drop);
        await ConsumeMaterial(danSamProject, DatabaseSeed.MaterialVallejoAcrylic, 45 + 20 + 50, MaterialUnit.Drop);

        await ConsumeMaterial(danSamProject, DatabaseSeed.MaterialAmmoTransparator, 50, MaterialUnit.Drop);
        await ConsumeMaterial(danSamProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 100, MaterialUnit.Spray);

        danSamProject.AddExecutionWindow(ProjectStepDefinition.Painting, referenceDate, 1 + 1 + 0.5 + 2 + 1.5 + 1 + 2 + 2 + 16);

        context.Add(danSamProject);

        #endregion

        #region Saint Seiya Diorama 

        referenceDate = referenceDate.AddDays(1);

        var saintSeyiaDiorama = new Project("rodrigo.basniak", "Saint Seiya Diorama", referenceDate, modelId: null);

        saintSeyiaDiorama.AddExecutionWindow(ProjectStepDefinition.Printing, referenceDate, 7.25 + 4 + 3.5 + 3.5 + 3 + 3.5 + 12 + 203);
        await ConsumeMaterial(saintSeyiaDiorama, DatabaseSeed.MaterialJayoAbsResin, (80 + 215 + 565), MaterialUnit.Gram);
        await ConsumeMaterial(saintSeyiaDiorama, DatabaseSeed.MaterialSunluWaterWashableStandardResin, (4760 + 185 + 55), MaterialUnit.Gram);

        saintSeyiaDiorama.AddExecutionWindow(ProjectStepDefinition.Planning, referenceDate, 3);
        saintSeyiaDiorama.AddExecutionWindow(ProjectStepDefinition.Supporting, referenceDate, 4 + 3 + 4 + 2 + 4 + 9);
        saintSeyiaDiorama.AddExecutionWindow(ProjectStepDefinition.Cleaning, referenceDate, 5);
        //saintSeyiaDiorama.AddExecutionWindow(ProjectStepDefinition.PostProcessing, referenceDate, 000000000000000);

        //await ConsumeMaterial(saintSeyiaDiorama, DatabaseSeed.MaterialPaintMixingCupCorrugated, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(saintSeyiaDiorama, DatabaseSeed.MaterialDisposableBrush, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(saintSeyiaDiorama, DatabaseSeed.MaterialDspiaeSandingDisk, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(saintSeyiaDiorama, DatabaseSeed.MaterialNailWoodSwab, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(saintSeyiaDiorama, DatabaseSeed.MaterialGlooves, 000000000000000, MaterialUnit.Unit);

        //await ConsumeMaterial(saintSeyiaDiorama, DatabaseSeed.MaterialMagnet10x5, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(saintSeyiaDiorama, DatabaseSeed.MaterialMagnet8x4, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(saintSeyiaDiorama, DatabaseSeed.MaterialMagnet6x4, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(saintSeyiaDiorama, DatabaseSeed.MaterialMagnet4x3, 000000000000000, MaterialUnit.Unit);

        //await ConsumeMaterial(saintSeyiaDiorama, DatabaseSeed.MaterialVallejoPrimer, 000000000000000, MaterialUnit.Drop);

        //await ConsumeMaterial(saintSeyiaDiorama, DatabaseSeed.MaterialArmyPainterAcrylic, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(saintSeyiaDiorama, DatabaseSeed.MaterialVallejoAcrylic, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(saintSeyiaDiorama, DatabaseSeed.MaterialAtomAcrylic, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(saintSeyiaDiorama, DatabaseSeed.MaterialAmmoAcrylic, 000000000000000, MaterialUnit.Drop);

        //await ConsumeMaterial(saintSeyiaDiorama, DatabaseSeed.MaterialGswVarnish, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(saintSeyiaDiorama, DatabaseSeed.MaterialAmmoTransparator, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(saintSeyiaDiorama, DatabaseSeed.MaterialVallejoLiquidMask, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(saintSeyiaDiorama, DatabaseSeed.MaterialBiltemaMatteVarnish, 000000000000000, MaterialUnit.Spray);

        //saintSeyiaDiorama.AddExecutionWindow(ProjectStepDefinition.Painting, referenceDate, 000000000000000);

        context.Add(saintSeyiaDiorama);

        #endregion

        #region Gandalf

        referenceDate = referenceDate.AddDays(1);

        var gandalfProject = new Project("rodrigo.basniak", "Gandalf", referenceDate, modelId: null);

        gandalfProject.AddExecutionWindow(ProjectStepDefinition.Printing, referenceDate, (160 + 215 + 170) / 15.0);
        await ConsumeMaterial(gandalfProject, DatabaseSeed.MaterialJayoAbsResin, (210 + 85 + 210 + 55 + 110 + 85), MaterialUnit.Gram);

        gandalfProject.AddExecutionWindow(ProjectStepDefinition.Planning, referenceDate, 1);
        gandalfProject.AddExecutionWindow(ProjectStepDefinition.Supporting, referenceDate, 3);
        gandalfProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, referenceDate, 0.5);
        //gandalfProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, referenceDate, 000000000000000);

        //await ConsumeMaterial(gandalfProject, DatabaseSeed.MaterialPaintMixingCupCorrugated, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(gandalfProject, DatabaseSeed.MaterialDisposableBrush, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(gandalfProject, DatabaseSeed.MaterialDspiaeSandingDisk, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(gandalfProject, DatabaseSeed.MaterialNailWoodSwab, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(gandalfProject, DatabaseSeed.MaterialGlooves, 000000000000000, MaterialUnit.Unit);

        //await ConsumeMaterial(gandalfProject, DatabaseSeed.MaterialMagnet10x5, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(gandalfProject, DatabaseSeed.MaterialMagnet8x4, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(gandalfProject, DatabaseSeed.MaterialMagnet6x4, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(gandalfProject, DatabaseSeed.MaterialMagnet4x3, 000000000000000, MaterialUnit.Unit);

        //await ConsumeMaterial(gandalfProject, DatabaseSeed.MaterialVallejoPrimer, 000000000000000, MaterialUnit.Drop);

        //await ConsumeMaterial(gandalfProject, DatabaseSeed.MaterialArmyPainterAcrylic, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(gandalfProject, DatabaseSeed.MaterialVallejoAcrylic, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(gandalfProject, DatabaseSeed.MaterialAtomAcrylic, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(gandalfProject, DatabaseSeed.MaterialAmmoAcrylic, 000000000000000, MaterialUnit.Drop);

        //await ConsumeMaterial(gandalfProject, DatabaseSeed.MaterialGswVarnish, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(gandalfProject, DatabaseSeed.MaterialAmmoTransparator, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(gandalfProject, DatabaseSeed.MaterialVallejoLiquidMask, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(gandalfProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 000000000000000, MaterialUnit.Spray);

        //gandalfProject.AddExecutionWindow(ProjectStepDefinition.Painting, referenceDate, 000000000000000);

        context.Add(gandalfProject);

        #endregion

        #region Legolas vs Troll 

        referenceDate = referenceDate.AddDays(1);

        var legolasDioramaProject = new Project("rodrigo.basniak", "Legolas vs Cave Troll ", referenceDate, modelId: null);

        legolasDioramaProject.AddExecutionWindow(ProjectStepDefinition.Printing, referenceDate, (5305) / 15.0);
        await ConsumeMaterial(legolasDioramaProject, DatabaseSeed.MaterialJayoAbsResin, (6410), MaterialUnit.Gram);

        legolasDioramaProject.AddExecutionWindow(ProjectStepDefinition.Planning, referenceDate, 6);
        legolasDioramaProject.AddExecutionWindow(ProjectStepDefinition.Supporting, referenceDate, 21.5 - 6);
        legolasDioramaProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, referenceDate, 10);
        //legolasDioramaProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, referenceDate, 000000000000000);

        //await ConsumeMaterial(legolasDioramaProject, DatabaseSeed.MaterialPaintMixingCupCorrugated, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(legolasDioramaProject, DatabaseSeed.MaterialDisposableBrush, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(legolasDioramaProject, DatabaseSeed.MaterialDspiaeSandingDisk, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(legolasDioramaProject, DatabaseSeed.MaterialNailWoodSwab, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(legolasDioramaProject, DatabaseSeed.MaterialGlooves, 000000000000000, MaterialUnit.Unit);

        //await ConsumeMaterial(legolasDioramaProject, DatabaseSeed.MaterialMagnet10x5, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(legolasDioramaProject, DatabaseSeed.MaterialMagnet8x4, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(legolasDioramaProject, DatabaseSeed.MaterialMagnet6x4, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(legolasDioramaProject, DatabaseSeed.MaterialMagnet4x3, 000000000000000, MaterialUnit.Unit);

        //await ConsumeMaterial(legolasDioramaProject, DatabaseSeed.MaterialVallejoPrimer, 000000000000000, MaterialUnit.Drop);

        //await ConsumeMaterial(legolasDioramaProject, DatabaseSeed.MaterialArmyPainterAcrylic, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(legolasDioramaProject, DatabaseSeed.MaterialVallejoAcrylic, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(legolasDioramaProject, DatabaseSeed.MaterialAtomAcrylic, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(legolasDioramaProject, DatabaseSeed.MaterialAmmoAcrylic, 000000000000000, MaterialUnit.Drop);

        //await ConsumeMaterial(legolasDioramaProject, DatabaseSeed.MaterialGswVarnish, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(legolasDioramaProject, DatabaseSeed.MaterialAmmoTransparator, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(legolasDioramaProject, DatabaseSeed.MaterialVallejoLiquidMask, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(legolasDioramaProject, DatabaseSeed.MaterialBiltemaMatteVarnish, 000000000000000, MaterialUnit.Spray);

        //legolasDioramaProject.AddExecutionWindow(ProjectStepDefinition.Painting, referenceDate, 000000000000000);

        context.Add(legolasDioramaProject);

        context.SaveChanges();

        #endregion

        //#region SAMPLE 
        //
        //referenceDate = referenceDate.AddDays(1);
        //
        //var sampleCode = new Project("rodrigo.basniak", "xxxxxxxxxxxxxxxxxxxxxx", referenceDate, modelId: null);

        //sampleCode.AddExecutionWindow(ProjectStepDefinition.Printing, referenceDate, (000000000000000) / 15.0);
        //await ConsumeMaterial(sampleCode, DatabaseSeed.MaterialJayoAbsResin, (000000000000000) , MaterialUnit.Gram); // As calculated in the slicer

        //sampleCode.AddExecutionWindow(ProjectStepDefinition.Planning, referenceDate, 000000000000000);
        //sampleCode.AddExecutionWindow(ProjectStepDefinition.Supporting, referenceDate, 000000000000000);
        //sampleCode.AddExecutionWindow(ProjectStepDefinition.Cleaning, referenceDate, 000000000000000);
        //sampleCode.AddExecutionWindow(ProjectStepDefinition.PostProcessing, referenceDate, 000000000000000);

        //await ConsumeMaterial(sampleCode, DatabaseSeed.MaterialPaintMixingCupCorrugated, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(sampleCode, DatabaseSeed.MaterialDisposableBrush, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(sampleCode, DatabaseSeed.MaterialDspiaeSandingDisk, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(sampleCode, DatabaseSeed.MaterialNailWoodSwab, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(sampleCode, DatabaseSeed.MaterialGlooves, 000000000000000, MaterialUnit.Unit);

        //await ConsumeMaterial(sampleCode, DatabaseSeed.MaterialMagnet10x5, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(sampleCode, DatabaseSeed.MaterialMagnet8x4, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(sampleCode, DatabaseSeed.MaterialMagnet6x4, 000000000000000, MaterialUnit.Unit);
        //await ConsumeMaterial(sampleCode, DatabaseSeed.MaterialMagnet4x3, 000000000000000, MaterialUnit.Unit);

        //await ConsumeMaterial(sampleCode, DatabaseSeed.MaterialVallejoPrimer, 000000000000000, MaterialUnit.Drop);

        //await ConsumeMaterial(sampleCode, DatabaseSeed.MaterialArmyPainterAcrylic, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(sampleCode, DatabaseSeed.MaterialVallejoAcrylic, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(sampleCode, DatabaseSeed.MaterialAtomAcrylic, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(sampleCode, DatabaseSeed.MaterialAmmoAcrylic, 000000000000000, MaterialUnit.Drop);

        //await ConsumeMaterial(sampleCode, DatabaseSeed.MaterialGswVarnish, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(sampleCode, DatabaseSeed.MaterialAmmoTransparator, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(sampleCode, DatabaseSeed.MaterialVallejoLiquidMask, 000000000000000, MaterialUnit.Drop);
        //await ConsumeMaterial(sampleCode, DatabaseSeed.MaterialBiltemaMatteVarnish, 000000000000000, MaterialUnit.Spray);

        //sampleCode.AddExecutionWindow(ProjectStepDefinition.Painting, referenceDate, 000000000000000);

        //context.Add(sampleCode);

        // context.SaveChanges();

        //#endregion
    }
}
