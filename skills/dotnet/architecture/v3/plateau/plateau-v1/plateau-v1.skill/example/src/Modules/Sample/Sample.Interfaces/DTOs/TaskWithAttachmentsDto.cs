namespace Sample.Interfaces.DTOs;

public record TaskWithAttachmentsDto(int Id, string Title, IReadOnlyList<AttachmentSummaryDto> Attachments);
