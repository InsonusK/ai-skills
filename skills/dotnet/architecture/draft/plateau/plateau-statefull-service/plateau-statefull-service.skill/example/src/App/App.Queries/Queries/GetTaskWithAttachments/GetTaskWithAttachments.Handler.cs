using App.Infrastructure.Persistence;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sample.Domain.Entities;
using Sample.Interfaces.DTOs;
using Sample.Interfaces.Queries;

namespace App.Queries.Queries.GetTaskWithAttachments;

public class GetTaskWithAttachmentsHandler(AppDbContext db)
    : IRequestHandler<GetTaskWithAttachmentsQuery, Result<TaskWithAttachmentsDto>>
{
    public async Task<Result<TaskWithAttachmentsDto>> Handle(GetTaskWithAttachmentsQuery query, CancellationToken ct)
    {
        var task = await db.Set<TaskItem>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == query.TaskId, ct);

        if (task is null)
            return Result.NotFound();

        var attachments = await db.Set<Attachment>()
            .AsNoTracking()
            .Where(a => a.TaskId == query.TaskId)
            .Select(a => new AttachmentSummaryDto(a.Id, a.Guid, a.FileName))
            .ToListAsync(ct);

        return Result.Success(new TaskWithAttachmentsDto(task.Id, task.Title, attachments));
    }
}
