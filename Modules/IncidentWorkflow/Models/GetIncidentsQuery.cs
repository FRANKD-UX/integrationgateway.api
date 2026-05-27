namespace IntegrationGateway.Api.Modules.IncidentWorkflow.Models;

public class GetIncidentsQuery
{
    public int? DepartmentId { get; set; }
    public int? ProjectId { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
}