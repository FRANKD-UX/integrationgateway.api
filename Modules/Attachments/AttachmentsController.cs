using Microsoft.AspNetCore.Mvc;

namespace IntegrationGateway.Api.Modules.Attachments
{
    [ApiController]
    [Route("api/incidents/{incidentId:int}/attachments")]
    public class AttachmentsController : ControllerBase
    {
        private const long MaxFileSizeBytes = 10 * 1024 * 1024;
        private const long MaxMultipartBodyBytes = 11 * 1024 * 1024;

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf",
            ".jpg",
            ".jpeg",
            ".png",
            ".docx",
            ".xlsx",
            ".txt"
        };

        private readonly AttachmentsService _attachmentsService;
        private readonly ILogger<AttachmentsController> _logger;

        public AttachmentsController(
            AttachmentsService attachmentsService,
            ILogger<AttachmentsController> logger)
        {
            _attachmentsService = attachmentsService;
            _logger = logger;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(MaxMultipartBodyBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxMultipartBodyBytes)]
        [ProducesResponseType(typeof(AttachmentUploadResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Upload(int incidentId, IFormFile? file)
        {
            if (file is null || file.Length == 0)
                return BadRequest(new { message = "Please select a file to upload." });

            if (file.Length > MaxFileSizeBytes)
                return BadRequest(new { message = "File size must be 10MB or smaller." });

            var extension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(extension))
            {
                return BadRequest(new
                {
                    message = "Unsupported file type. Allowed types are pdf, jpg, jpeg, png, docx, xlsx, and txt."
                });
            }

            try
            {
                var uploaded = await _attachmentsService.UploadAsync(incidentId, file);
                return Ok(uploaded);
            }
            catch (AttachmentStorageException ex)
            {
                _logger.LogWarning(ex, "Attachment upload failed for incident {IncidentId}", incidentId);
                return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<AttachmentListItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetAttachments(int incidentId)
        {
            try
            {
                var attachments = await _attachmentsService.GetAttachmentsAsync(incidentId);
                return Ok(attachments);
            }
            catch (AttachmentStorageException ex)
            {
                _logger.LogWarning(ex, "Attachment list fetch failed for incident {IncidentId}", incidentId);
                return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
            }
        }

        [HttpDelete("{fileId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Delete(int incidentId, string fileId)
        {
            try
            {
                await _attachmentsService.DeleteAsync(incidentId, fileId);
                return NoContent();
            }
            catch (AttachmentStorageException ex)
            {
                _logger.LogWarning(ex,
                    "Attachment delete failed for incident {IncidentId}, file {FileId}",
                    incidentId,
                    fileId);
                return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
            }
        }
    }
}
