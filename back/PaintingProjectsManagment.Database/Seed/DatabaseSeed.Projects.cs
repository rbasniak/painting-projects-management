using PaintingProjectsManagement.Features.Projects;
using System.Diagnostics;

namespace PaintingProjectsManagment.Database;

public partial class DatabaseSeed
{

    private void ProjectsSeed(DatabaseContext context, IServiceProvider provider)
    {
        async Task ConsumeMaterial(Project project, string materialName, double quantity, MaterialUnit unit)
        {
            ReadOnlyMaterial? material = null;

            var timeout = DateTime.UtcNow.AddSeconds(5);

            do
            {
                material = context.Set<ReadOnlyMaterial>().FirstOrDefault(x => x.Name == materialName);

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

        ConsumeMaterial(archangelProject, _materialMagnet10x5, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(archangelProject, _materialMagnet5x3, 3, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(archangelProject, _materialMagnet3x2, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(archangelProject, _materialSunluWaterWashableStandardResin, 165 + 120 + 75 + 25 + 95 + 60, MaterialUnit.Gram).GetAwaiter().GetResult();
        ConsumeMaterial(archangelProject, _materialVallejoPrimer, 200, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(archangelProject, _materialGswPrimer, 200, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(archangelProject, _materialGswChrome, 375 + 50, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(archangelProject, _materialArmyPainterAcrylic, 50 + 25 + 10 + 10 + 20 + 10 + 10 + 15 + 5 + 10 + 5 + 5 + 25 + 25 + 50, MaterialUnit.Drop).GetAwaiter().GetResult();

        archangelProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (218.0 + 135.0 + 110.0) / 20.0);

        archangelProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 4);
        archangelProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 1.5);
        archangelProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 1.5); 
        archangelProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 0.5+2+13);
        archangelProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 28), 15);
        archangelProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 08, 10), 8);

        context.Add(archangelProject);

        context.SaveChanges();

        #endregion

        #region Illidan

        var illidanProject = new Project("rodrigo.basniak", "Illidan Stormrage", new DateTime(2025, 08, 15), modelId: null);

        ConsumeMaterial(illidanProject, _materialMagnet10x5, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(illidanProject, _materialMagnet8x4, 18, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(illidanProject, _materialMagnet3x2, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        
        ConsumeMaterial(illidanProject, _materialPaintMixingCupCorrugated, 10, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(illidanProject, _materialMaskingTape3m25mm, 3, MaterialUnit.Meter).GetAwaiter().GetResult();
        ConsumeMaterial(illidanProject, _materialVallejoLiquidMask, 2, MaterialUnit.Mililiter).GetAwaiter().GetResult();
        ConsumeMaterial(illidanProject, _materialDisposableBrush, 2, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(illidanProject, _materialSunluWaterWashableStandardResin, (210 + 160 + 205), MaterialUnit.Gram).GetAwaiter().GetResult();

        ConsumeMaterial(illidanProject, _materialVallejoPrimer, 150, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(illidanProject, _materialBiltemaMatteVarnish, 68+19+30, MaterialUnit.Spray).GetAwaiter().GetResult();
        ConsumeMaterial(illidanProject, _materialGswVarnish, 50, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(illidanProject, _materialArmyPainterAcrylic, 345+50+25+115+50+75, MaterialUnit.Drop).GetAwaiter().GetResult();

        illidanProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (260.0 + 206.0 + 204.0) / 20.0);

        illidanProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 6);
        illidanProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 1.5);
        illidanProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 2);
        illidanProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 0.5+6+3+1+3+3+1.5+3);

        context.Add(illidanProject);

        context.SaveChanges();

        #endregion

        #region Sylvanas

        var sylvanasProject = new Project("rodrigo.basniak", "Sylvanas Windrunner", new DateTime(2025, 08, 15), modelId: null);

        ConsumeMaterial(sylvanasProject, _materialMagnet10x5, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(sylvanasProject, _materialMagnet8x4, 16, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(sylvanasProject, _materialMagnet6x4, 10, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(sylvanasProject, _materialMagnet3x2, 8, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(sylvanasProject, _materialSunluWaterWashableStandardResin, (200 + 235 + 150), MaterialUnit.Gram).GetAwaiter().GetResult();

        ConsumeMaterial(sylvanasProject, _materialVallejoPrimer, 225, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(sylvanasProject, _materialBiltemaMatteVarnish, 75, MaterialUnit.Spray).GetAwaiter().GetResult();
        ConsumeMaterial(sylvanasProject, _materialGswAcrylic, 125+50+5+15, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(sylvanasProject, _materialGswChrome, 100, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(sylvanasProject, _materialTamiyaTS80, 15, MaterialUnit.Spray).GetAwaiter().GetResult();
        ConsumeMaterial(sylvanasProject, _materialArmyPainterAcrylic, 10+20+50+10+15+25+50+50+10, MaterialUnit.Drop).GetAwaiter().GetResult();

        sylvanasProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (223.0 + 200.0 + 115.0) / 20.0);

        sylvanasProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 6);
        sylvanasProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 1.5);
        sylvanasProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 2);
        sylvanasProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 0.5+1.5+5+6+6+1.5+1.5);

        context.Add(sylvanasProject);

        context.SaveChanges();

        #endregion

        #region Lara Croft

        var laraProject = new Project("rodrigo.basniak", "Lara Croft", new DateTime(2025, 08, 15), modelId: null);

        ConsumeMaterial(laraProject, _materialMagnet10x5, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(laraProject, _materialMagnet8x4, 14, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(laraProject, _materialMagnet6x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(laraProject, _materialTamiyaTS80, 70+5+15+50, MaterialUnit.Spray).GetAwaiter().GetResult();

        ConsumeMaterial(laraProject, _materialMaskingTapeTamiya10mm, 30, MaterialUnit.Centimeter).GetAwaiter().GetResult();

        laraProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (105.0 + 177.0 + 186.0) / 20.0);
        ConsumeMaterial(laraProject, _materialSunluWaterWashableStandardResin, (180.0 + 165.5 + 195.0), MaterialUnit.Gram).GetAwaiter().GetResult();

        ConsumeMaterial(laraProject, _materialVallejoPrimer, 175, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(laraProject, _materialArmyPainterAcrylic, 25+95+150+30+75+50+40+155, MaterialUnit.Drop).GetAwaiter().GetResult();

        laraProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 6);
        laraProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 1);
        laraProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 3);
        laraProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 0.5+1+0.5+3+1+15+2+2);

        context.Add(laraProject);

        context.SaveChanges();

        #endregion

        #region Camus of Aquarius

        var camusProject = new Project("rodrigo.basniak", "Camus of Aquarius", new DateTime(2025, 08, 15), modelId: null);

        camusProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (190.0+210.0+185.0+145.0+135.0) / 20.0);
        ConsumeMaterial(camusProject, _materialSunluWaterWashableStandardResin, (175+125+335+60+150+45+160+50+130+75), MaterialUnit.Gram).GetAwaiter().GetResult();

        ConsumeMaterial(camusProject, _materialMagnet10x5, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(camusProject, _materialMagnet8x4, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(camusProject, _materialMagnet6x4, 10, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(camusProject, _materialBiltemaMatteVarnish, 92+65+35, MaterialUnit.Spray).GetAwaiter().GetResult();

        ConsumeMaterial(camusProject, _materialVallejoPrimer, 100+100+100, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(camusProject, _materialArmyPainterAcrylic, 100+75+15+25+25+125+50+50+25, MaterialUnit.Drop).GetAwaiter().GetResult();

        camusProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 4);
        camusProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 1.5);
        camusProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 4.5);
        camusProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 1+3+2+2+1.5+2+4);

        context.Add(camusProject);

        context.SaveChanges();

        #endregion

        #region Kratos

        var kratosProject = new Project("rodrigo.basniak", "Kratos", new DateTime(2025, 08, 15), modelId: null);

        ConsumeMaterial(kratosProject, _materialMagnet10x5, 34, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, _materialMagnet8x4, 9, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, _materialMagnet6x4, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, _materialDspiaeSandingDisk, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, _materialModellingPutty, 5, MaterialUnit.Gram).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, _materialMaskingTapeGeneric5mm, 1, MaterialUnit.Meter).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, _materialDisposableBrush, 5, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, _materialPaintMixingCupCorrugated, 10, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, _materialPaintMixingCupNonCorrugated, 5, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, _materialMaskingTapeGeneric10mm, 200, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, _materialMaskingTape3m25mm, 100, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, _materialArmyPainterSnow, 150*0.75, MaterialUnit.Gram).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, _materialMaskingTapeTamiya20mm, 100, MaterialUnit.Centimeter).GetAwaiter().GetResult();

        kratosProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (160+160+155+155+135+125+190) / 20.0);
        ConsumeMaterial(kratosProject, _materialSunluAbsResin, 195+75+225+65+195+70+135+55+165+60+155+60+150+80, MaterialUnit.Gram).GetAwaiter().GetResult();

        ConsumeMaterial(kratosProject, _materialVallejoPrimer, 175+375+50+15, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, _materialVallejoVarnish, 500+25+25, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, _materialGswVarnish, 25, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, _materialBiltemaMatteVarnish, 110+40+105, MaterialUnit.Spray).GetAwaiter().GetResult();

        ConsumeMaterial(kratosProject, _materialArmyPainterAcrylic, 245+640, MaterialUnit.Drop).GetAwaiter().GetResult();

        kratosProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 9.5);
        kratosProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 2);
        kratosProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 9);
        kratosProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 1.75+4.5+2+4+10+10+8+7.5+5);

        context.Add(kratosProject);

        context.SaveChanges();

        #endregion

        #region Starlight

        var starligtProject = new Project("rodrigo.basniak", "Starlight", new DateTime(2025, 08, 15), modelId: null);

        ConsumeMaterial(starligtProject, _materialMagnet10x5, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(starligtProject, _materialMagnet8x4, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(starligtProject, _materialMagnet6x4, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(starligtProject, _materialMagnet3x2, 4, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(starligtProject, _materialTamiyaTS80, 25+25+25+20, MaterialUnit.Spray).GetAwaiter().GetResult();

        ConsumeMaterial(starligtProject, _materialDisposableBrush, 2, MaterialUnit.Unit).GetAwaiter().GetResult();

        starligtProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (115.0 + 85.0) / 20.0);
        ConsumeMaterial(starligtProject, _materialJayoAbsResin, 150+90+40, MaterialUnit.Gram).GetAwaiter().GetResult();

        ConsumeMaterial(starligtProject, _materialVallejoPrimer, 50, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(starligtProject, _materialArmyPainterAcrylic, 50+50+50+20+25+40+5+20+145+75+10+25+10, MaterialUnit.Drop).GetAwaiter().GetResult();

        starligtProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 2.5);
        starligtProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 1.5);
        starligtProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 1);
        starligtProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 0.5 + 1+3+1.5+2+2);

        context.Add(starligtProject);

        context.SaveChanges();

        #endregion

        #region Harley Quinn 

        var harleyProject = new Project("rodrigo.basniak", "Harley Quinn", new DateTime(2025, 08, 15), modelId: null);

        ConsumeMaterial(harleyProject, _materialMagnet10x5, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(harleyProject, _materialMagnet8x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(harleyProject, _materialMagnet6x4, 6, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(harleyProject, _materialVallejoPrimer, 150, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(harleyProject, _materialArmyPainterAcrylic, 40+75+40+425+45, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(harleyProject, _materialTamiyaTS80, 125+30+35+45+45, MaterialUnit.Spray).GetAwaiter().GetResult();
        ConsumeMaterial(harleyProject, _materialBiltemaMatteVarnish, 25, MaterialUnit.Spray).GetAwaiter().GetResult();

        ConsumeMaterial(harleyProject, _materialDisposableBrush, 2, MaterialUnit.Unit).GetAwaiter().GetResult();
        
        ConsumeMaterial(harleyProject, _materialMaskingTape3m25mm, 60, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(harleyProject, _materialMaskingTapeGeneric25mm, 40, MaterialUnit.Centimeter).GetAwaiter().GetResult();

        harleyProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (152+105) / 20.0);
        ConsumeMaterial(harleyProject, _materialSunluAbsResin, 235+115, MaterialUnit.Gram).GetAwaiter().GetResult();


        harleyProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 3);
        harleyProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 0.5);
        harleyProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 2);
        harleyProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 1+2.5+8+9+2.5+6+2.5+1+1.5);

        context.Add(harleyProject);

        context.SaveChanges();

        #endregion

        #region Sadie Adler

        var sadieProject = new Project("rodrigo.basniak", "Sadie Adler", new DateTime(2025, 08, 15), modelId: null);

        ConsumeMaterial(sadieProject, _materialMagnet10x5, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(sadieProject, _materialMagnet8x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(sadieProject, _materialMagnet6x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(sadieProject, _materialMagnet5x3, 4, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(sadieProject, _materialVallejoPrimer, 150, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(sadieProject, _materialArmyPainterAcrylic, 10+30+90+5+3+15+20+20+5+15+5+15, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(sadieProject, _materialVallejoAcrylic, 45, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(sadieProject, _materialAtomAcrylic, 10+7+25+2+5+20+30, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(sadieProject, _materialDisposableBrush, 1, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(sadieProject, _materialDspiaeSandingDisk, 1, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(sadieProject, _materialMaskingTapeTamiya10mm, 20, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(sadieProject, _materialMaskingTapeGeneric25mm, 20, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(sadieProject, _materialMaskingTapeGeneric10mm, 20+15, MaterialUnit.Centimeter).GetAwaiter().GetResult();

        sadieProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (70+150) / 20.0);
        ConsumeMaterial(sadieProject, _materialJayoAbsResin, (75+35+100+45), MaterialUnit.Gram).GetAwaiter().GetResult();

        sadieProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 3);
        sadieProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 1);
        sadieProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 1.5);
        sadieProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 0.5+0.5+13+2);

        context.Add(sadieProject);

        context.SaveChanges();

        #endregion

        #region Hulkbuster and Iron Man

        var hulkbusterProject = new Project("rodrigo.basniak", "Hulkbuster and Iron Man", new DateTime(2025, 08, 15), modelId: null);

        hulkbusterProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (156 + 143 + 145 + 153 + 138 + 135 + 165 + 115 + 132 + 148 + 131 + 150 + 90 + 136 + 96 + 98) / 17.5);
        ConsumeMaterial(hulkbusterProject, _materialJayoAbsResin, (185+175+220+160+175+165+200+185+180+185+215+210+170+235+95+105), MaterialUnit.Gram).GetAwaiter().GetResult();

        ConsumeMaterial(hulkbusterProject, _materialMagnet10x5, 10, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, _materialMagnet8x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, _materialDspiaeSandingDisk, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, _materialDisposableBrush, 10, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, _materialPaintMixingCupCorrugated, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, _materialGlooves, 25, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, _materialNailWoodSwab, 50, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(hulkbusterProject, _materialGswPrimer, 1040, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, _materialVallejoPrimer, 215, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, _materialAkChrome, 115, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, _materialGswChrome, 50+100+100+100+39+100+100+100+75, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, _materialVallejoAcrylic, 10, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, _materialTamiyaClear, 75+50+40+200+6+20+250+250, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, _materialAmmoCandy, 75, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(hulkbusterProject, _materialArmyPainterAcrylic, 45+50+40+10, MaterialUnit.Drop).GetAwaiter().GetResult();

        hulkbusterProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 12), 4);
        hulkbusterProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 12), 2+2+3.5+1.5+12+4);
        hulkbusterProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 12), 6);
        hulkbusterProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 12), 7.5);
        hulkbusterProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 12), 3+1+6+4.5+2+5+12+2+1.5+1);

        context.Add(hulkbusterProject);

        context.SaveChanges();

        #endregion

        #region Harry Potter

        var harryProject = new Project("rodrigo.basniak", "Harry Potter", new DateTime(2025, 08, 15), modelId: null);

        harryProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (140+190+58+140) / 20);
        ConsumeMaterial(harryProject, _materialJayoAbsResin, 115 + 30 + 185 + 55 + 51 + 25 + 190 + 85, MaterialUnit.Gram).GetAwaiter().GetResult();

        harryProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 12), 1);
        harryProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 12), 5);
        harryProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 12), 2);
        harryProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 12), 1.5);

        ConsumeMaterial(harryProject, _materialMagnet10x5, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, _materialMagnet8x4, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, _materialMaskingTapeTamiya10mm, 10+15, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, _materialMaskingTapeGeneric25mm, 15+25, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, _materialMaskingTapeGeneric10mm, 100, MaterialUnit.Centimeter).GetAwaiter().GetResult();

        ConsumeMaterial(harryProject, _materialVallejoPrimer, 300, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, _materialGswChrome, 30, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, _materialArmyPainterAcrylic, 100+25+50+5+25+20+25+5+5+5+5+5+5+5+5+10, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, _materialVallejoAcrylic, 5+15+10+5+25+10+15, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, _materialBiltemaMatteVarnish, 50, MaterialUnit.Spray).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, _materialAtomAcrylic, 30+10+5+4+7, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, _materialAmmoAcrylic, 5+40+10+10, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, _materialVallejoVarnish, 30+55, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, _materialAmmoDrybrush, 5, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, _materialAmmoTransparator, 130+40+10+15, MaterialUnit.Drop).GetAwaiter().GetResult();

        harryProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 12), 0.5+5+1+1.5+6.5+5+1);

        context.Add(harryProject);

        context.SaveChanges();

        #endregion

        #region Raven

        var ravenProject = new Project("rodrigo.basniak", "Raven", new DateTime(2025, 11, 01), modelId: null);

        ravenProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (190+210) / 20);
        ConsumeMaterial(ravenProject, _materialJayoAbsResin, (170+55+195+50), MaterialUnit.Gram).GetAwaiter().GetResult();

        ravenProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        ravenProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 5);
        ravenProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 1);
        ravenProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 2);

        ConsumeMaterial(ravenProject, _materialMagnet10x5, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, _materialMagnet8x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, _materialMagnet4x3, 2, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, _materialMagnet3x2, 1, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, _materialMagnet6x4, 2, MaterialUnit.Unit).GetAwaiter().GetResult();
        
        ConsumeMaterial(ravenProject, _materialMaskingTape3m25mm, 15, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, _materialMaskingTapeGeneric25mm, 10, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, _materialMaskingTapeTamiya10mm, 30, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, _materialDisposableBrush, 2, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, _materialNailWoodSwab, 35, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(ravenProject, _materialVallejoPrimer, 250, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, _materialArmyPainterAcrylic, 25+10+5+50+30+20+15+15+20, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, _materialVallejoAcrylic, 5+10+10, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, _materialAmmoDrybrush, 5, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, _materialAtomAcrylic, 18, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, _materialAmmoTransparator, 30, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, _materialTamiyaTS80, 55, MaterialUnit.Spray).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, _materialBiltemaMatteVarnish, 25, MaterialUnit.Spray).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, _materialVallejoVarnish, 15, MaterialUnit.Drop).GetAwaiter().GetResult();

        ravenProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 3+4+6+0.5);

        context.Add(ravenProject);

        context.SaveChanges();

        #endregion

        #region Lace

        var laceProject = new Project("rodrigo.basniak", "Lady Lace", new DateTime(2025, 11, 01), modelId: null);

        laceProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (200 + 30) / 20);
        ConsumeMaterial(laceProject, _materialJayoAbsResin, (200 + 40), MaterialUnit.Gram).GetAwaiter().GetResult();

        laceProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        laceProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 1.5);
        laceProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 0.5);
        laceProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 0);

        ConsumeMaterial(laceProject, _materialMagnet10x5, 8, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(laceProject, _materialVallejoPrimer, 250, MaterialUnit.Drop).GetAwaiter().GetResult();

        //ConsumeMaterial(laceProject, _materialArmyPainterAcrylic, 245 + 640, MaterialUnit.Drop).GetAwaiter().GetResult();
        //ConsumeMaterial(laceProject, _materialArmyPainterAcrylic, 245 + 640, MaterialUnit.Drop).GetAwaiter().GetResult();
        //ConsumeMaterial(laceProject, _materialArmyPainterAcrylic, 245 + 640, MaterialUnit.Drop).GetAwaiter().GetResult();
        //ConsumeMaterial(laceProject, _materialArmyPainterAcrylic, 245 + 640, MaterialUnit.Drop).GetAwaiter().GetResult();

        // laceProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 0.0);

        context.Add(laceProject);

        context.SaveChanges();

        #endregion

        #region Aphrodite

        var aphroditeProject = new Project("rodrigo.basniak", "Aphrodite of Pisces", new DateTime(2025, 12, 10), modelId: null);

        aphroditeProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (90+210+215+210+120+205+200+155+140) / 17);
        ConsumeMaterial(aphroditeProject, _materialJayoAbsResin, 110+50+160+70+220+105+160+105, MaterialUnit.Gram).GetAwaiter().GetResult();
        ConsumeMaterial(aphroditeProject, _materialAnycubricClearResin, 70+70+90+50+180+85+115+45+85+55, MaterialUnit.Gram).GetAwaiter().GetResult();

        aphroditeProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 2);
        aphroditeProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 18);
        aphroditeProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 4.5);
        aphroditeProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 1); // must be updated

        ConsumeMaterial(aphroditeProject, _materialMagnet10x5, 12, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(aphroditeProject, _materialMagnet8x4, 7, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(aphroditeProject, _materialMagnet6x4, 2, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(aphroditeProject, _materialVallejoPrimer, 0, MaterialUnit.Drop).GetAwaiter().GetResult(); // must be updated
        ConsumeMaterial(aphroditeProject, _materialGswPrimer, 0, MaterialUnit.Drop).GetAwaiter().GetResult(); // must be updated
        ConsumeMaterial(aphroditeProject, _materialTamiyaClear, 0, MaterialUnit.Drop).GetAwaiter().GetResult(); // must be updated
        ConsumeMaterial(aphroditeProject, _materialAmmoCandy, 0, MaterialUnit.Drop).GetAwaiter().GetResult(); // must be updated
        ConsumeMaterial(aphroditeProject, _materialArmyPainterAcrylic, 0, MaterialUnit.Drop).GetAwaiter().GetResult(); // must be updated

        ConsumeMaterial(aphroditeProject, _materialBiltemaGlossVarnish, 30+70+30+67, MaterialUnit.Spray).GetAwaiter().GetResult(); // must be updated

        aphroditeProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 0.0); // must be updated

        context.Add(aphroditeProject);

        context.SaveChanges();

        #endregion

        #region Iron Man and Tony Stark

        var tonyStarkProject = new Project("rodrigo.basniak", "Iton Man and Tony Stark", new DateTime(2025, 01, 01), modelId: null);

        tonyStarkProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (200+125+90)/13);
        ConsumeMaterial(tonyStarkProject, _materialSunluWaterWashableStandardResin, 325+115+170+65+80+40, MaterialUnit.Gram).GetAwaiter().GetResult();

        tonyStarkProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        tonyStarkProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 6);
        tonyStarkProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 1);
        tonyStarkProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 2);

        ConsumeMaterial(tonyStarkProject, _materialMagnet10x5, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(tonyStarkProject, _materialMagnet8x4, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(tonyStarkProject, _materialMagnet6x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(tonyStarkProject, _materialVallejoPrimer, 75+135, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(tonyStarkProject, _materialArmyPainterAcrylic, 35+20+10+5+15+10+5+10+60+5+10+5+30+30+10+10, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(tonyStarkProject, _materialTamiyaClear, 20, MaterialUnit.Mililiter).GetAwaiter().GetResult();

        tonyStarkProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 1.5+1+1.5+0.5+2+1.5+1.5+3+2+3.5+2+3+2+2);

        context.Add(tonyStarkProject);

        context.SaveChanges();

        #endregion

        #region Kassandra

        var kassandraProject = new Project("rodrigo.basniak", "Kassandra", new DateTime(2025, 01, 01), modelId: null);

        kassandraProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (155+105+215+175)/15);
        ConsumeMaterial(kassandraProject, _materialSunluWaterWashableStandardResin, 245+60+70+35+155+100+250, MaterialUnit.Gram).GetAwaiter().GetResult();

        kassandraProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        kassandraProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 6);
        kassandraProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 1);
        kassandraProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 2);

        ConsumeMaterial(kassandraProject, _materialMagnet10x5, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(kassandraProject, _materialMagnet8x4, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(kassandraProject, _materialMagnet6x4, 2, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(kassandraProject, _materialMagnet3x2, 12, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(kassandraProject, _materialVallejoPrimer, 23, MaterialUnit.Mililiter).GetAwaiter().GetResult();

        ConsumeMaterial(kassandraProject, _materialArmyPainterAcrylic, 15+40+10+10+5+70+40+2+30+5+30+15+10+310, MaterialUnit.Drop).GetAwaiter().GetResult();

        kassandraProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 1.5+1+1.5+1.5+0.25+1.25+2+1.5+1+3+7+3+5+2);

        context.Add(kassandraProject);

        context.SaveChanges();

        #endregion

        #region Rogue vs Miss Marvel

        var missMarvelProject = new Project("rodrigo.basniak", "Rogue vs Miss Marvel", new DateTime(2025, 01, 01), modelId: null);

        missMarvelProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (160+90+200)/15.0);
        ConsumeMaterial(missMarvelProject, _materialSunluWaterWashableStandardResin, 155+70+100+50+135+75, MaterialUnit.Gram).GetAwaiter().GetResult();

        missMarvelProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        missMarvelProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 6);
        missMarvelProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 1);
        missMarvelProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 3);

        ConsumeMaterial(missMarvelProject, _materialMagnet10x5, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(missMarvelProject, _materialMagnet8x4, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(missMarvelProject, _materialMagnet6x4, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(missMarvelProject, _materialMagnet3x2, 4, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(missMarvelProject, _materialVallejoPrimer, 225, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(missMarvelProject, _materialArmyPainterAcrylic, 60+10+15+15+30+20+10+10+5+5+5+45+20+20+45+20+20+20+5+50+100+35+10+40+10+40+20+20, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(missMarvelProject, _materialVallejoVarnish, 150, MaterialUnit.Drop).GetAwaiter().GetResult();

        missMarvelProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 1.5+0.25+0.25+2+1.5+2+4.5+5+12);

        context.Add(missMarvelProject);

        context.SaveChanges();

        #endregion

        #region Yennefer

        var yenneferProject = new Project("rodrigo.basniak", "Yennefer", new DateTime(2025, 01, 01), modelId: null);

        yenneferProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (85+140) / 15.0);
        ConsumeMaterial(yenneferProject, _materialSunluWaterWashableStandardResin, 95+20+210+35, MaterialUnit.Gram).GetAwaiter().GetResult();

        yenneferProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        yenneferProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 4);
        yenneferProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 0.5);
        yenneferProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 1);

        ConsumeMaterial(yenneferProject, _materialMagnet6x4, 12, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(yenneferProject, _materialVallejoPrimer, 75, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(yenneferProject, _materialArmyPainterAcrylic, 10+10+20+20+15+15+10+10+5+5+5+5+30+10+50+5+5+10+10+5+10, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(yenneferProject, _materialVallejoVarnish, 75, MaterialUnit.Drop).GetAwaiter().GetResult();

        yenneferProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 25);

        context.Add(yenneferProject);

        context.SaveChanges();

        #endregion

        #region Wonder Woman

        var wonderWomanProject = new Project("rodrigo.basniak", "Wonder Woman", new DateTime(2025, 01, 01), modelId: null);

        wonderWomanProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (145+155) / 15.0);
        ConsumeMaterial(wonderWomanProject, _materialSunluWaterWashableStandardResin, 130+60+150+45, MaterialUnit.Gram).GetAwaiter().GetResult();

        wonderWomanProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        wonderWomanProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 4);
        wonderWomanProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 0.5);
        wonderWomanProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 1.5);

        ConsumeMaterial(wonderWomanProject, _materialMagnet10x5, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(wonderWomanProject, _materialMagnet8x4, 12, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(wonderWomanProject, _materialMagnet6x4, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(wonderWomanProject, _materialMagnet5x3, 4, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(wonderWomanProject, _materialVallejoPrimer, 200+325, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(wonderWomanProject, _materialArmyPainterAcrylic, 15+15+30+9+6+7+435, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(wonderWomanProject, _materialVallejoAcrylic, 000000000, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(wonderWomanProject, _materialAtomAcrylic, 000000000, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(wonderWomanProject, _materialVallejoVarnish, 000000000, MaterialUnit.Drop).GetAwaiter().GetResult();

        wonderWomanProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 25);

        context.Add(wonderWomanProject);

        context.SaveChanges();

        #endregion

        #region Black Widow

        var blackWidowsProject = new Project("rodrigo.basniak", "Black Widow", new DateTime(2025, 01, 01), modelId: null);

        blackWidowsProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (120) / 15.0);
        ConsumeMaterial(blackWidowsProject, _materialSunluWaterWashableStandardResin, 150+60, MaterialUnit.Gram).GetAwaiter().GetResult();

        blackWidowsProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 0);
        blackWidowsProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 2);
        blackWidowsProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 0.25);
        blackWidowsProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 1);

        ConsumeMaterial(blackWidowsProject, _materialMagnet8x4, 10, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(blackWidowsProject, _materialMagnet6x4, 6, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(blackWidowsProject, _materialVallejoPrimer, 150+15+10, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(blackWidowsProject, _materialArmyPainterAcrylic, 15+5+5+5+30+30+10+50+10+30, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(blackWidowsProject, _materialVallejoVarnish, 60, MaterialUnit.Drop).GetAwaiter().GetResult();

        blackWidowsProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 0.5+1+1+1+4+9+2);

        context.Add(blackWidowsProject);

        context.SaveChanges();

        #endregion

        #region Lightning

        var lightningProject = new Project("rodrigo.basniak", "Lightning", new DateTime(2025, 01, 01), modelId: null);

        lightningProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (130+110) / 15.0);
        ConsumeMaterial(lightningProject, _materialSunluWaterWashableStandardResin, 100+40+80+30, MaterialUnit.Gram).GetAwaiter().GetResult();

        lightningProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        lightningProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 4);
        lightningProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 0.5);
        lightningProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 2);

        ConsumeMaterial(lightningProject, _materialMagnet10x5, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(lightningProject, _materialMagnet8x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(lightningProject, _materialMagnet6x4, 8, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(lightningProject, _materialVallejoPrimer, 550, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(lightningProject, _materialArmyPainterAcrylic, 240+50+75+15+70, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(lightningProject, _materialVallejoVarnish, 100, MaterialUnit.Drop).GetAwaiter().GetResult();

        lightningProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 20);

        context.Add(lightningProject);

        context.SaveChanges();

        #endregion

        #region Thrall

        var thrallProject = new Project("rodrigo.basniak", "Thrall", new DateTime(2025, 01, 01), modelId: null);

        thrallProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (130+90+125) / 15.0);
        ConsumeMaterial(thrallProject, _materialSunluWaterWashableStandardResin, 145+60+85+40+95+35, MaterialUnit.Gram).GetAwaiter().GetResult();

        thrallProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        thrallProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 6);
        thrallProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 1);
        thrallProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 2.5);

        ConsumeMaterial(thrallProject, _materialMagnet10x5, 16, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(thrallProject, _materialMagnet8x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(thrallProject, _materialVallejoPrimer, 450, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(thrallProject, _materialArmyPainterAcrylic, 30+15+30+10+10+15+5+25+15+25+10+25+20+40+30+10, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(thrallProject, _materialVallejoVarnish, 200, MaterialUnit.Drop).GetAwaiter().GetResult();

        thrallProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 1+1+3+2+2+6+4+2);

        context.Add(thrallProject);

        context.SaveChanges();

        #endregion

        #region Donkey Kong

        var DonkeyKongProject = new Project("rodrigo.basniak", "Donkey Kong", new DateTime(2025, 01, 01), modelId: null);

        DonkeyKongProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (85+175+100) / 15.0);
        ConsumeMaterial(DonkeyKongProject, _materialSunluWaterWashableStandardResin, 110+30+250+90+70+20, MaterialUnit.Gram).GetAwaiter().GetResult();

        DonkeyKongProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        DonkeyKongProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 5);
        DonkeyKongProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 1);
        DonkeyKongProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 6);

        ConsumeMaterial(DonkeyKongProject, _materialMagnet6x4, 20, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(DonkeyKongProject, _materialMagnet8x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(DonkeyKongProject, _materialMagnet3x2, 4, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(DonkeyKongProject, _materialVallejoPrimer, 450, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(DonkeyKongProject, _materialArmyPainterAcrylic, 15+5+5+3+5+120+5+10+5+5+5+5+20+6+6+20+20+5+5+5+20+15+50+15+15+30+25+75, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(DonkeyKongProject, _materialVallejoVarnish, 200, MaterialUnit.Drop).GetAwaiter().GetResult();

        DonkeyKongProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 2.5+1+1+1.5+3+2+2+8+4);

        context.Add(DonkeyKongProject);

        context.SaveChanges();

        #endregion

        #region Final Fantasy Diorama

        var ffxivProject = new Project("rodrigo.basniak", "Final Fantasy Diorama", new DateTime(2025, 01, 01), modelId: null);

        ffxivProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (175+165+160+165+105+85) / 15.0);
        ConsumeMaterial(ffxivProject, _materialSunluWaterWashableStandardResin, 140+80+190+45+175+55+170+55+85+35+95+40, MaterialUnit.Gram).GetAwaiter().GetResult();

        ffxivProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        ffxivProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 10);
        ffxivProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 1.5);
        ffxivProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 4);

        ConsumeMaterial(ffxivProject, _materialMagnet10x5, 12, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(ffxivProject, _materialMagnet8x4, 16, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(ffxivProject, _materialMagnet6x4, 15, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(ffxivProject, _materialVallejoPrimer, 400, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(ffxivProject, _materialArmyPainterAcrylic, 720, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(ffxivProject, _materialVallejoVarnish, 300, MaterialUnit.Drop).GetAwaiter().GetResult();

        ffxivProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 34);

        context.Add(ffxivProject);

        context.SaveChanges();

        #endregion

        #region Yunna

        var yunnaProject = new Project("rodrigo.basniak", "Yunna", new DateTime(2025, 01, 01), modelId: null);

        yunnaProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (100+150) / 15.0);
        ConsumeMaterial(yunnaProject, _materialSunluWaterWashableStandardResin, 75+45+75+35, MaterialUnit.Gram).GetAwaiter().GetResult();

        yunnaProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        yunnaProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 4);
        yunnaProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 0.5);
        yunnaProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 2);

        ConsumeMaterial(yunnaProject, _materialMagnet10x5, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(yunnaProject, _materialMagnet8x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(yunnaProject, _materialMagnet6x4, 6, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(yunnaProject, _materialVallejoPrimer, 300, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(yunnaProject, _materialArmyPainterAcrylic, 250, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(yunnaProject, _materialVallejoVarnish, 150, MaterialUnit.Drop).GetAwaiter().GetResult();

        yunnaProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 23);

        context.Add(yunnaProject);

        context.SaveChanges();

        #endregion

        #region Makima

        var newProject = new Project("rodrigo.basniak", "Makima", new DateTime(2025, 01, 01), modelId: null);

        newProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (160) / 17);
        ConsumeMaterial(newProject, _materialJayoAbsResin, 100+60, MaterialUnit.Gram).GetAwaiter().GetResult();

        newProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 0.5);
        newProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 2);
        newProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 0.5);
        newProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 1);

        ConsumeMaterial(newProject, _materialMagnet10x5, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(newProject, _materialMagnet8x4, 2, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(newProject, _materialVallejoPrimer, 000000000, MaterialUnit.Drop).GetAwaiter().GetResult(); 

        ConsumeMaterial(newProject, _materialArmyPainterAcrylic, 000000000, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(newProject, _materialVallejoVarnish, 000000000, MaterialUnit.Drop).GetAwaiter().GetResult();

        newProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 000000000);

        context.Add(newProject);

        context.SaveChanges();

        #endregion

        //#region SAMPLE 

        //var newProject = new Project("rodrigo.basniak", "New_Project", new DateTime(2025, 01, 01), modelId: null);

        //newProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (000000000) / 15.0);
        //ConsumeMaterial(newProject, _materialSunluWaterWashableStandardResin, 000000000, MaterialUnit.Gram).GetAwaiter().GetResult();

        //newProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        //newProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 000000000);
        //newProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 000000000);
        //newProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 000000000);

        //ConsumeMaterial(newProject, _materialMagnet10x5, 000000000, MaterialUnit.Unit).GetAwaiter().GetResult();
        //ConsumeMaterial(newProject, _materialMagnet8x4, 000000000, MaterialUnit.Unit).GetAwaiter().GetResult();
        //ConsumeMaterial(newProject, _materialMagnet6x4, 000000000, MaterialUnit.Unit).GetAwaiter().GetResult();

        //ConsumeMaterial(newProject, _materialVallejoPrimer, 000000000, MaterialUnit.Drop).GetAwaiter().GetResult();

        //ConsumeMaterial(newProject, _materialArmyPainterAcrylic, 000000000, MaterialUnit.Drop).GetAwaiter().GetResult();
        //ConsumeMaterial(newProject, _materialVallejoAcrylic, 000000000, MaterialUnit.Drop).GetAwaiter().GetResult();
        //ConsumeMaterial(newProject, _materialAtomAcrylic, 000000000, MaterialUnit.Drop).GetAwaiter().GetResult();

        //ConsumeMaterial(newProject, _materialVallejoVarnish, 000000000, MaterialUnit.Drop).GetAwaiter().GetResult();

        //newProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 000000000);

        //context.Add(newProject);

        //context.SaveChanges();

        //#endregion
    }
}
