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

            var timeout = DateTime.UtcNow.AddSeconds(60);

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

        ConsumeMaterial(archangelProject, _materialMagnet10x5, 2, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(archangelProject, _materialMagnet5x3, 3, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(archangelProject, _materialSunluWaterWashableStandardResin, 160 + 120 + 75 + 25 + 95 + 60, MaterialUnit.Gram).GetAwaiter().GetResult();
        ConsumeMaterial(archangelProject, _materialVallejoPrimer, 200, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(archangelProject, _materialGswPrimer, 200, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(archangelProject, _materialGswChrome, 375 + 50, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(archangelProject, _materialArmyPainterAcrylic, 50 + 25 + 10 + 10 + 20 + 10 + 10 + 15 + 5 + 10 + 5 + 5 + 25 + 25 + 50, MaterialUnit.Drop).GetAwaiter().GetResult();

        archangelProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (218.0 + 135.0 + 110.0) / 20.0);

        archangelProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 4);
        archangelProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 1.5);
        archangelProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 3.5); 
        archangelProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 0.5 + 2 + 13);
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

        ConsumeMaterial(illidanProject, _materialSunluWaterWashableStandardResin, (210 + 160 + 205) * 1.5, MaterialUnit.Gram).GetAwaiter().GetResult();

        ConsumeMaterial(illidanProject, _materialVallejoPrimer, 150, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(illidanProject, _materialBiltemaMatteVarnish, 68+19+30, MaterialUnit.Unit).GetAwaiter().GetResult();
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

        ConsumeMaterial(sylvanasProject, _materialSunluWaterWashableStandardResin, (200 + 235 + 150) * 1.5, MaterialUnit.Gram).GetAwaiter().GetResult();

        ConsumeMaterial(sylvanasProject, _materialVallejoPrimer, 225, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(sylvanasProject, _materialBiltemaMatteVarnish, 75, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(sylvanasProject, _materialGswAcrylic, 125+50+5+15, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(sylvanasProject, _materialGswChrome, 100, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(sylvanasProject, _materialTamiyaTS80, 15, MaterialUnit.Unit).GetAwaiter().GetResult();
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

        ConsumeMaterial(laraProject, _materialTamiyaTS80, 70+5+15+50, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(laraProject, _materialMaskingTapeTamiya10mm, 30, MaterialUnit.Centimeter).GetAwaiter().GetResult();

        laraProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (105.0 + 177.0 + 186.0) / 20.0);
        ConsumeMaterial(laraProject, _materialSunluWaterWashableStandardResin, (180.0 + 165.5 + 195.0) * 1.5, MaterialUnit.Gram).GetAwaiter().GetResult();

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
        ConsumeMaterial(camusProject, _materialSunluWaterWashableStandardResin, (175+125+335+60+150+45+160+50+130+75) * 1.5, MaterialUnit.Gram).GetAwaiter().GetResult();

        ConsumeMaterial(camusProject, _materialMagnet10x5, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(camusProject, _materialMagnet8x4, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(camusProject, _materialMagnet6x4, 10, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(camusProject, _materialBiltemaMatteVarnish, 92+65+35, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(camusProject, _materialVallejoPrimer, 100+100+100, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(camusProject, _materialArmyPainterAcrylic, 100+75+15+25+25+125+50+50, MaterialUnit.Drop).GetAwaiter().GetResult();

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

        kratosProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (105.0 + 177.0 + 186.0) / 20.0);
        ConsumeMaterial(kratosProject, _materialSunluWaterWashableStandardResin, (180.0 + 165.5 + 195.0) * 1.5, MaterialUnit.Gram).GetAwaiter().GetResult();

        ConsumeMaterial(kratosProject, _materialVallejoPrimer, 175+375+50+15, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, _materialVallejoVarnish, 500+25+25, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, _materialGswVarnish, 50, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(kratosProject, _materialBiltemaMatteVarnish, 110+40+105, MaterialUnit.Unit).GetAwaiter().GetResult();

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

        ConsumeMaterial(starligtProject, _materialTamiyaTS80, 25+25+25+20, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(starligtProject, _materialDisposableBrush, 2, MaterialUnit.Centimeter).GetAwaiter().GetResult();

        starligtProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (115.0 + 85.0) / 20.0);
        ConsumeMaterial(starligtProject, _materialJayoAbsResin, (150+90+40) * 1.5, MaterialUnit.Gram).GetAwaiter().GetResult();

        ConsumeMaterial(starligtProject, _materialVallejoPrimer, 50, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(starligtProject, _materialArmyPainterAcrylic, 50+50+50+20+25+40+5+20+145+75+10+25+10, MaterialUnit.Drop).GetAwaiter().GetResult();

        starligtProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 2.5);
        starligtProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 1.5);
        starligtProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 1);
        starligtProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 0.5 + 1+3+1.5+2+2);

        context.Add(starligtProject);

        context.SaveChanges();

        #endregion

        // FALTA TACO E OLHOS
        #region Harley Quinn 

        var harleyProject = new Project("rodrigo.basniak", "Harley Quinn", new DateTime(2025, 08, 15), modelId: null);

        ConsumeMaterial(harleyProject, _materialMagnet10x5, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(harleyProject, _materialMagnet8x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(harleyProject, _materialMagnet6x4, 6, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(harleyProject, _materialVallejoPrimer, 150, MaterialUnit.Drop).GetAwaiter().GetResult();
        ConsumeMaterial(harleyProject, _materialArmyPainterAcrylic, 40+75+40+425+45, MaterialUnit.Drop).GetAwaiter().GetResult();

        ConsumeMaterial(harleyProject, _materialTamiyaTS80, 25 + 25 + 25 + 20, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(harleyProject, _materialBiltemaMatteVarnish, 25, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(harleyProject, _materialDisposableBrush, 2, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        
        ConsumeMaterial(harleyProject, _materialMaskingTape3m25mm, 60, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(harleyProject, _materialMaskingTapeGeneric25mm, 40, MaterialUnit.Centimeter).GetAwaiter().GetResult();

        harleyProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (152+105) / 20.0);
        ConsumeMaterial(harleyProject, _materialJayoAbsResin, (235+115) * 1.5, MaterialUnit.Gram).GetAwaiter().GetResult();


        harleyProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 3);
        harleyProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 0.5);
        harleyProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 2);
        harleyProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 1+2.5+8+9+0.5+2+6+2.5);

        context.Add(harleyProject);

        context.SaveChanges();

        #endregion

        // FALTA OLHOS E PELE
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

        ConsumeMaterial(sadieProject, _materialDisposableBrush, 1, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(sadieProject, _materialDspiaeSandingDisk, 1, MaterialUnit.Centimeter).GetAwaiter().GetResult();

        ConsumeMaterial(sadieProject, _materialMaskingTapeTamiya10mm, 20, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(sadieProject, _materialMaskingTapeGeneric25mm, 20, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(sadieProject, _materialMaskingTapeGeneric10mm, 20+15, MaterialUnit.Centimeter).GetAwaiter().GetResult();

        sadieProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (70+150) / 20.0);
        ConsumeMaterial(sadieProject, _materialJayoAbsResin, (75+35+100+45) * 1.5, MaterialUnit.Gram).GetAwaiter().GetResult();

        sadieProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 3);
        sadieProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 06, 26), 1);
        sadieProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 06, 26), 1.5);
        sadieProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 0.5+0.5+13);

        context.Add(sadieProject);

        context.SaveChanges();

        #endregion

        #region Hulkbuster and Iron Man

        var hulkbusterProject = new Project("rodrigo.basniak", "Hulkbuster and Iron Man", new DateTime(2025, 08, 15), modelId: null);

        hulkbusterProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (156 + 143 + 145 + 153 + 138 + 135 + 165 + 115 + 132 + 148 + 131 + 150 + 90 + 136 + 96 + 98) / 17.5);
        ConsumeMaterial(hulkbusterProject, _materialSunluWaterWashableStandardResin, (185+175+220+160+175+165+200+185+180+185+215+210+170+235+95+105) * 1.5, MaterialUnit.Gram).GetAwaiter().GetResult();

        //ConsumeMaterial(hulkbusterProject, _materialMagnet10x5, 34, MaterialUnit.Unit).GetAwaiter().GetResult();
        //ConsumeMaterial(hulkbusterProject, _materialMagnet8x4, 9, MaterialUnit.Unit).GetAwaiter().GetResult();
        //ConsumeMaterial(hulkbusterProject, _materialMagnet6x4, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        //ConsumeMaterial(hulkbusterProject, _materialDspiaeSandingDisk, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        //ConsumeMaterial(hulkbusterProject, _materialModellingPutty, 5, MaterialUnit.Gram).GetAwaiter().GetResult();
        //ConsumeMaterial(hulkbusterProject, _materialMaskingTapeGeneric5mm, 1, MaterialUnit.Meter).GetAwaiter().GetResult();
        //ConsumeMaterial(hulkbusterProject, _materialDisposableBrush, 5, MaterialUnit.Unit).GetAwaiter().GetResult();
        //ConsumeMaterial(hulkbusterProject, _materialPaintMixingCupCorrugated, 10, MaterialUnit.Unit).GetAwaiter().GetResult();
        //ConsumeMaterial(hulkbusterProject, _materialPaintMixingCupNonCorrugated, 5, MaterialUnit.Unit).GetAwaiter().GetResult();
        //ConsumeMaterial(hulkbusterProject, _materialMaskingTapeGeneric10mm, 200, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        //ConsumeMaterial(hulkbusterProject, _materialMaskingTape3m25mm, 100, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        //ConsumeMaterial(hulkbusterProject, _materialArmyPainterSnow, 150 * 0.75, MaterialUnit.Gram).GetAwaiter().GetResult();
        //ConsumeMaterial(hulkbusterProject, _materialMaskingTapeTamiya20mm, 100, MaterialUnit.Centimeter).GetAwaiter().GetResult();

        //ConsumeMaterial(hulkbusterProject, _materialVallejoPrimer, 175375 + 50 + 15, MaterialUnit.Drop).GetAwaiter().GetResult();
        //ConsumeMaterial(hulkbusterProject, _materialVallejoVarnish, 500 + 25 + 25, MaterialUnit.Drop).GetAwaiter().GetResult();
        //ConsumeMaterial(hulkbusterProject, _materialGswVarnish, 50, MaterialUnit.Drop).GetAwaiter().GetResult();
        //ConsumeMaterial(hulkbusterProject, _materialBiltemaMatteVarnish, 110 + 40 + 105, MaterialUnit.Unit).GetAwaiter().GetResult();

        //ConsumeMaterial(hulkbusterProject, _materialArmyPainterAcrylic, 245 + 640, MaterialUnit.Drop).GetAwaiter().GetResult();

        hulkbusterProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 12), 4);
        hulkbusterProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 12), 2+2+3.5+1.5+12+4);
        hulkbusterProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 12), 6);
        // hulkbusterProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 12), 0.0);
        // hulkbusterProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 12), 0.0);

        context.Add(hulkbusterProject);

        context.SaveChanges();

        #endregion

        #region Harry Potter

        var harryProject = new Project("rodrigo.basniak", "Harry Potter", new DateTime(2025, 08, 15), modelId: null);

        harryProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 08, 10), (140+190+58+140) / 20);
        ConsumeMaterial(harryProject, _materialSunluWaterWashableStandardResin, (115+30+185+55+51+25+190+85) * 1.5, MaterialUnit.Gram).GetAwaiter().GetResult();

        harryProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 12), 1);
        harryProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 12), 5);
        harryProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 12), 2);
        harryProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 12), 1.5);

        ConsumeMaterial(harryProject, _materialMagnet10x5, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, _materialMagnet8x4, 6, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, _materialMaskingTapeTamiya10mm, 10, MaterialUnit.Centimeter).GetAwaiter().GetResult();
        ConsumeMaterial(harryProject, _materialMaskingTapeGeneric25mm, 15, MaterialUnit.Centimeter).GetAwaiter().GetResult();

        ConsumeMaterial(harryProject, _materialVallejoPrimer, 300, MaterialUnit.Drop).GetAwaiter().GetResult();

        //ConsumeMaterial(harryProject, _materialArmyPainterAcrylic, 245 + 640, MaterialUnit.Drop).GetAwaiter().GetResult();
        //ConsumeMaterial(harryProject, _materialArmyPainterAcrylic, 245 + 640, MaterialUnit.Drop).GetAwaiter().GetResult();
        //ConsumeMaterial(harryProject, _materialArmyPainterAcrylic, 245 + 640, MaterialUnit.Drop).GetAwaiter().GetResult();
        //ConsumeMaterial(harryProject, _materialArmyPainterAcrylic, 245 + 640, MaterialUnit.Drop).GetAwaiter().GetResult();

        // harryProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 12), 0.0);

        context.Add(harryProject);

        context.SaveChanges();

        #endregion

        #region Raven

        var ravenProject = new Project("rodrigo.basniak", "Raven", new DateTime(2025, 11, 01), modelId: null);

        ravenProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (190+210) / 20);
        ConsumeMaterial(ravenProject, _materialSunluWaterWashableStandardResin, (170+55+195+50) * 1.5, MaterialUnit.Gram).GetAwaiter().GetResult();

        ravenProject.AddExecutionWindow(ProjectStepDefinition.Planning, new DateTime(2025, 11, 01), 1);
        ravenProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 11, 01), 5);
        ravenProject.AddExecutionWindow(ProjectStepDefinition.Cleaning, new DateTime(2025, 11, 01), 1);
        ravenProject.AddExecutionWindow(ProjectStepDefinition.PostProcessing, new DateTime(2025, 11, 01), 2);

        ConsumeMaterial(ravenProject, _materialMagnet10x5, 8, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, _materialMagnet8x4, 4, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, _materialMagnet4x3, 2, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, _materialMagnet3x2, 1, MaterialUnit.Unit).GetAwaiter().GetResult();
        ConsumeMaterial(ravenProject, _materialMagnet6x4, 2, MaterialUnit.Unit).GetAwaiter().GetResult();

        ConsumeMaterial(ravenProject, _materialVallejoPrimer, 250, MaterialUnit.Drop).GetAwaiter().GetResult();

        //ConsumeMaterial(ravenProject, _materialArmyPainterAcrylic, 245 + 640, MaterialUnit.Drop).GetAwaiter().GetResult();
        //ConsumeMaterial(ravenProject, _materialArmyPainterAcrylic, 245 + 640, MaterialUnit.Drop).GetAwaiter().GetResult();
        //ConsumeMaterial(ravenProject, _materialArmyPainterAcrylic, 245 + 640, MaterialUnit.Drop).GetAwaiter().GetResult();
        //ConsumeMaterial(ravenProject, _materialArmyPainterAcrylic, 245 + 640, MaterialUnit.Drop).GetAwaiter().GetResult();

        // ravenProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 11, 01), 0.0);

        context.Add(ravenProject);

        context.SaveChanges();

        #endregion

        #region Lace

        var laceProject = new Project("rodrigo.basniak", "Lady Lace", new DateTime(2025, 11, 01), modelId: null);

        laceProject.AddExecutionWindow(ProjectStepDefinition.Printing, new DateTime(2025, 11, 01), (200 + 30) / 20);
        ConsumeMaterial(laceProject, _materialSunluWaterWashableStandardResin, (200 + 40) * 1.5, MaterialUnit.Gram).GetAwaiter().GetResult();

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
    }
}
