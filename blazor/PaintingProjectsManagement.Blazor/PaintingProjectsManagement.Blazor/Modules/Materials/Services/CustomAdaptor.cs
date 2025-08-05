using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;
using System.Collections;
using System.Diagnostics;

namespace PaintingProjectsManagement.Blazor.Modules.Materials;

public class CustomAdaptor : DataAdaptor
{
    private readonly IMaterialsService _materialsService;
    public CustomAdaptor(IMaterialsService materialsService)
    {
        _materialsService = materialsService;
    }
    
    public List<MaterialDetails> Materials { get; set; }  
    
    public override async Task<Object> ReadAsync(DataManagerRequest dataManagerRequest, string key = null)
    {
        var materials = await _materialsService.GetMaterialsAsync(default);

        Console.WriteLine($"Loaded {materials.Count} materisl");
        Debug.WriteLine($"Loaded {materials.Count} materisl");

        Materials = materials.ToList();

        IEnumerable GridData = Materials;
        IEnumerable DataSource = Materials;

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
        Materials.Insert(0, value as MaterialDetails);
        return value;
    }
    public override async Task<object> RemoveAsync(DataManager dataManager, object value, string keyField, string key)
    {
        var data = (Guid)value;
        Materials.Remove(Materials.Where(x => x.Id == data).FirstOrDefault());
        return value;
    }
    public override async Task<object> UpdateAsync(DataManager dataManager, object value, string keyField, string key)
    {
        var val = (value as MaterialDetails);
        var data = Materials.Where(x => x.Id == val.Id).FirstOrDefault();
        if (data != null)
        {
            data.Name = val.Name;
            data.Unit = val.Unit;
            data.PricePerUnit = val.PricePerUnit;
        }
        return value;
    }
}