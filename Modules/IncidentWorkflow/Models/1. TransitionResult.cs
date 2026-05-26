namespace IntegrationGateway.Api.Modules.IncidentWorkflow.Models;

public class TransitionResult
{
	public bool Success { get; set; }
	public string? ErrorMessage { get; set; }
	public IncidentDto? Incident { get; set; }

	public static TransitionResult Ok(IncidentDto incident) => new()
	{
		Success = true,
		Incident = incident
	};

	public static TransitionResult Fail(string reason) => new()
	{
		Success = false,
		ErrorMessage = reason
	};
}