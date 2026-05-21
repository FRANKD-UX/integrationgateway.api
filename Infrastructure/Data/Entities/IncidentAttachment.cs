using System;
using System.Collections.Generic;

namespace IntegrationGateway.Api.Infrastructure.Data.Entities;

public partial class IncidentAttachment
{
    public int AttachmentId { get; set; }

    public int IncidentId { get; set; }

    public int? StepId { get; set; }

    public string FileName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public long? FileSize { get; set; }

    public string? ContentType { get; set; }

    public string? AttachmentType { get; set; }

    public int? UploadedByUserId { get; set; }

    public DateTime UploadedAt { get; set; }

    public virtual Incident Incident { get; set; } = null!;

    public virtual IncidentChainStep? Step { get; set; }
}
