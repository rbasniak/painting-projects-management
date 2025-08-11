using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;
using System.Collections;
using System.Diagnostics;

namespace PaintingProjectsManagement.Blazor.Modules.Materials;

public class MaterialsAdaptor : DataAdaptor
{
    private readonly IMaterialsService _materialsService;
    public MaterialsAdaptor(IMaterialsService materialsService)
    {
        _materialsService = materialsService;
    }
    
    public List<MaterialDetails> Materials { get; set; }  
    
    public override async Task<Object> ReadAsync(DataManagerRequest dataManagerRequest, string key = null)
    {
        var materials = await _materialsService.GetMaterialsAsync(default);

        Console.WriteLine($"Loaded {materials.Count} materials");

        Materials = materials.ToList();

        IEnumerable GridData = Materials;
        IEnumerable DataSource = Materials;

        if (dataManagerRequest.Where != null && dataManagerRequest.Where.Count > 0)
        {
            GridData = DataOperations.PerformFiltering(GridData, dataManagerRequest.Where, dataManagerRequest.Where[0].Operator);
        }
        if (dataManagerRequest.Sorted?.Count > 0) // perform Sorting
        {
            GridData = DataOperations.PerformSorting(GridData, dataManagerRequest.Sorted);
        }
        if (dataManagerRequest.Skip != 0)
        {
            GridData = DataOperations.PerformSkip(GridData, dataManagerRequest.Skip); //Paging
        }
        if (dataManagerRequest.Take != 0)
        {
            GridData = DataOperations.PerformTake(GridData, dataManagerRequest.Take);
        }
        IDictionary<string, object> aggregates = new Dictionary<string, object>();
        if (dataManagerRequest.Aggregates != null) // Aggregation
        {
            aggregates = DataUtil.PerformAggregation(DataSource, dataManagerRequest.Aggregates);
        }
        if (dataManagerRequest.Group != null && dataManagerRequest.Group.Any()) //Grouping
        {
            foreach (var group in dataManagerRequest.Group)
            {
                GridData = DataUtil.Group<MaterialDetails>(GridData, group, dataManagerRequest.Aggregates, 0, dataManagerRequest.GroupByFormatter);
            }
        }
        return dataManagerRequest.RequiresCounts ? new DataResult() { Result = GridData, Count = Materials.Count(), Aggregates = aggregates } : (object)GridData;
    }
    public override async Task<Object> InsertAsync(DataManager dataManager, object value, string key)
    {
        var newMaterialData = value as MaterialDetails;

        var newMaterial = await _materialsService.CreateMaterialAsync(new CreateMaterialRequest
        {
            Name = newMaterialData.Name,
            PricePerUnit = newMaterialData.PricePerUnit,
            Unit = (MaterialUnit)newMaterialData.Unit.Id
        }, default);

        Materials.Insert(0, newMaterial);
        return value;
    }
    public override async Task<object> RemoveAsync(DataManager dataManager, object value, string keyField, string key)
    {
        var id = (Guid)value;

        await _materialsService.DeleteMaterialAsync(id, default);

        Materials.Remove(Materials.Where(x => x.Id == id).First());

        return value;
    }
    public override async Task<object> UpdateAsync(DataManager dataManager, object value, string keyField, string key)
    {
        var updateData = (value as MaterialDetails);

        var updatedMaterial = await _materialsService.UpdateMaterialAsync(new UpdateMaterialRequest
        {
            Id = updateData.Id,
            Name = updateData.Name,
            PricePerUnit = updateData.PricePerUnit,
            Unit = (MaterialUnit)updateData.Unit.Id
        }, default);

        var index = Materials.FindIndex(x => x.Id == updateData.Id);

        Materials[index] = updatedMaterial;

        return value;
    }
}