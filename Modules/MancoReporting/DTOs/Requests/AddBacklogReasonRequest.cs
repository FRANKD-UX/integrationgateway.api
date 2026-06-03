using System.ComponentModel.DataAnnotations;

namespace IntegrationGateway.Api.Modules.MancoReporting.DTOs.Requests;

public record AddBacklogReasonRequest([property: Required, MaxLength(1024)] string ReasonText);
