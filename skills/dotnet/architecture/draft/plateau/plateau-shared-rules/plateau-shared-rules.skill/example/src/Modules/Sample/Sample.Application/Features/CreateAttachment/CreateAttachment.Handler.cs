using Ardalis.Result;
using MediatR;
using Sample.Domain.Entities;
using Sample.Interfaces.Commands;
using Shared.Repositories;

namespace Sample.Application.Features.CreateAttachment;

public class CreateAttachmentHandler(IRepository<Attachment> repository)
    : IRequestHandler<CreateAttachmentCommand, Result<CreateAttachmentResult>>
{
    public async Task<Result<CreateAttachmentResult>> Handle(CreateAttachmentCommand command, CancellationToken cancellationToken)
    {
        var attachment = new Attachment(command.Guid, command.TaskId, command.FileName);
        attachment.SetCreationInfo(command.ActionTimeStamp);

        await repository.AddAsync(attachment, cancellationToken);

        return Result.Created(new CreateAttachmentResult());
    }
}
