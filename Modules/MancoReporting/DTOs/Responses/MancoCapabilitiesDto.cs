namespace IntegrationGateway.Api.Modules.MancoReporting.DTOs.Responses;

public record MancoCapabilitiesDto(
    bool CanCreateProject,
    bool CanSubmitReport,
    bool CanAddBacklogReason,
    bool CanUpdateTaskStatus,
    bool CanReadComments,
    bool CanReadPriorityHistory,
    bool CanCreateComment,
    bool CanResolveComment,
    bool CanSetPriority,
    bool CanReviewReport,
    bool CanActionReport,
    bool CanReadAllProjects);
