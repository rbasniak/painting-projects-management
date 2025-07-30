using System.Text.Json.Serialization;

namespace PaintingProjectsManagement.Features.Projects;

public class ProjectStepData 
{
    [JsonConstructor]
    private ProjectStepData(Guid id, DateTime startDate, DateTime? endDate)
    {
        Id = id;
        StartDate = startDate;
        EndDate = endDate;
    }

    public ProjectStepData()
    { 
    }

    public ProjectStepData(DateTime startDate)
    {
        Id = Guid.NewGuid();
        StartDate = startDate;
    }

    public Guid Id { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public double Duration => (EndDate - StartDate)?.TotalHours ?? 0;
}