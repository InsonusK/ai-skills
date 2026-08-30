using Ardalis.Result;
using MediatR;
using Sample.Application.Specifications;
using Sample.Domain.Entities;
using Sample.Interfaces.Commands;
using Shared.Repositories;

namespace Sample.Application.Features.DeleteAttachment;

public class DeleteAttachmentHandler(IRepository<Attachment> repository)
    : IRequestHandler<DeleteAttachmentCommand, Result>
{
    public async Task<Result> Handle(DeleteAttachmentCommand command, CancellationToken ct)
    {
        var attachment = await repository.FirstOrDefaultAsync(
            new AttachmentByTaskAndGuidSpec(command.TaskId, command.AttachmentGuid), ct);
        if (attachment is null)
            return Result.NotFound();

        await repository.DeleteAsync(attachment, ct);

        return Result.Success();
    }
}
