using PaintingProjectsManagement.Features.Projects;

namespace PaintingProjectsManagment.Database;

public partial class DatabaseSeed
{

    private void ProjectsSeed(DatabaseContext context, IServiceProvider provider)
    {
        //void AddMaterial(Project project, string materialName,  quantity, MaterialUnit unit)
        //{
        //    var material = context.Set<Material>().First(x => x.Name == materialName);
        //    project.ConsumeMaterial(material.Id, quantity, unit);
        //}
        //var magnet10x5 = context.Set<Material>().First(x => x.Name == "Magnet 10x5");
        //var magnet5x3 = context.Set<Material>().First(x => x.Name == "Magnet 5x3");
        //var sunluStandardResin = context.Set<Material>().First(x => x.Name == "Sunlu Standard Resin");
        //var sunluAbsLikeClearResin = context.Set<Material>().First(x => x.Name == "Sunlu ABS-like Clear Resin");
        //var primer = context.Set<Material>().First(x => x.Name == "Primer");
        //var chromePaint = context.Set<Material>().First(x => x.Name == "Green Stuff World Chrome Paints");
        //var paint = context.Set<Material>().First(x => x.Name == "Army Painter Warpaint Fanatics");

        //var archangelProject = new Project("rodrigo.basniak", "Archangel", new DateTime(2025, 07, 15), model.Id);
        //archangelProject.ConsumeMaterial(magnet10x5.Id, 2, MaterialUnit.Unit);
        //archangelProject.ConsumeMaterial(magnet5x3.Id, 3, MaterialUnit.Unit);
        //archangelProject.ConsumeMaterial(sunluStandardResin.Id, 160 + 120, MaterialUnit.Grams);
        //archangelProject.ConsumeMaterial(sunluAbsLikeClearResin.Id, 75 + 25 + 95 + 60, MaterialUnit.Grams);
        //archangelProject.ConsumeMaterial(primer.Id, 200 + 150 + 50, MaterialUnit.Drops);
        //archangelProject.ConsumeMaterial(chromePaint.Id, 375 + 50, MaterialUnit.Drops);
        //archangelProject.ConsumeMaterial(paint.Id, 50 + 25 + 10 + 10 + 20 + 10 + 10 + 15 + 5 + 10 + 5 + 5 + 25 + 25 + 50, MaterialUnit.Drops);

        //archangelProject.AddExecutionWindow(ProjectStepDefinition.Supporting, new DateTime(2025, 06, 24), 4);
        //archangelProject.AddExecutionWindow(ProjectStepDefinition.Preparation, new DateTime(2025, 06, 26), 1.5 + 3.5);
        //archangelProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 27), 0.5 + 2 + 13);
        //archangelProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 06, 28), 15);
        //archangelProject.AddExecutionWindow(ProjectStepDefinition.Painting, new DateTime(2025, 08, 10), 8);

        //archangelProject.SetTotalPrintingHeight(218 + 135 + 110);

        //context.Add(archangelProject);

        //context.SaveChanges();
    }
}
