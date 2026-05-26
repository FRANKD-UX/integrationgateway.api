using IntegrationGateway.Api.Services;
using System.Net.Http.Headers;

namespace IntegrationGateway.Api.Modules.IncidentWorkflow;

public enum NotificationType
{
    NewIncident,
    HandedOff,
    ReturnedToOrigin,
    SlaReminder,
    Escalation,
    Closed
}

public class NotificationService
{
    private readonly IncidentWorkflowRepository _repository;
    private readonly GraphAuthService _graphAuthService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IncidentWorkflowRepository repository,
        GraphAuthService graphAuthService,
        IConfiguration configuration,
        ILogger<NotificationService> logger)
    {
        _repository = repository;
        _graphAuthService = graphAuthService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task NotifyDepartmentAsync(
        int incidentId,
        int departmentId,
        string message,
        NotificationType type)
    {
        var users = await _repository
            .GetDepartmentUsersAsync(departmentId);

        foreach (var user in users.Where(u => !string.IsNullOrEmpty(u.Email)))
        {
            await SendEmailAsync(
                user.Email!,
                GetSubject(type, incidentId),
                message,
                incidentId
            );
        }
    }

    public async Task NotifyAllInvolvedAsync(
        int incidentId,
        string message)
    {
        var incident = await _repository
            .GetIncidentWithChainAsync(incidentId);

        if (incident is null)
            return;

        var involvedDepartmentIds = incident.IncidentChainSteps
            .Select(s => s.DepartmentId)
            .Distinct()
            .ToList();

        foreach (var deptId in involvedDepartmentIds)
        {
            await NotifyDepartmentAsync(
                incidentId,
                deptId,
                message,
                NotificationType.Closed
            );
        }
    }

    private async Task SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        int incidentId)
    {
        try
        {
            var accessToken = await _graphAuthService.GetAccessTokenAsync();
            var senderEmail = _configuration["Graph:SenderEmail"];

            var emailPayload = new
            {
                message = new
                {
                    subject = subject,
                    body = new
                    {
                        contentType = "HTML",
                        content = BuildEmailBody(body, incidentId)
                    },
                    toRecipients = new[]
                    {
                        new
                        {
                            emailAddress = new
                            {
                                address = toEmail
                            }
                        }
                    }
                },
                saveToSentItems = false
            };

            var json = System.Text.Json.JsonSerializer.Serialize(emailPayload);
            var content = new StringContent(
                json,
                System.Text.Encoding.UTF8,
                "application/json"
            );

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.PostAsync(
                $"https://graph.microsoft.com/v1.0/users/{senderEmail}/sendMail",
                content
            );

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Email sent to {Email} for incident {IncidentId}",
                    toEmail,
                    incidentId
                );
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Failed to send email to {Email}. Status: {Status}. Error: {Error}",
                    toEmail,
                    response.StatusCode,
                    error
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Exception sending email to {Email} for incident {IncidentId}",
                toEmail,
                incidentId
            );
        }
    }

    private static string GetSubject(
        NotificationType type,
        int incidentId) => type switch
        {
            NotificationType.NewIncident =>
                $"New Incident #{incidentId} Assigned to Your Department",
            NotificationType.HandedOff =>
                $"Incident #{incidentId} Has Been Handed Off to Your Department",
            NotificationType.ReturnedToOrigin =>
                $"Incident #{incidentId} Returned for Sign Off",
            NotificationType.SlaReminder =>
                $"Reminder: Incident #{incidentId} Requires Action",
            NotificationType.Escalation =>
                $"ESCALATION: Incident #{incidentId} Has Breached SLA",
            NotificationType.Closed =>
                $"Incident #{incidentId} Has Been Closed",
            _ => $"Incident #{incidentId} Update"
        };

    private static string BuildEmailBody(
        string message,
        int incidentId) => $"""
        <html>
        <body style="font-family: Arial, sans-serif; padding: 20px;">
            <h2 style="color: #0078d4;">Incident Management System</h2>
            <p>{message}</p>
            <p>
                <strong>Incident Reference:</strong> #{incidentId}
            </p>
            <p style="color: #666; font-size: 12px;">
                This is an automated notification.
                Please log in to the portal to take action.
            </p>
        </body>
        </html>
        """;
}