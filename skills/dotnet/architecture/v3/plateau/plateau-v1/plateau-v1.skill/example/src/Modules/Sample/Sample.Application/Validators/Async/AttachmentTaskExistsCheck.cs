using FluentValidation;
using Sample.Application.Specifications;
using Sample.Domain.Entities;
using Sample.Interfaces.Commands;
using Shared.Repositories;

namespace Sample.Application.Validators.Async;

public sealed class AttachmentTaskExistsCheck(IReadRepository<TaskItem> taskRepository)
{
    private async Task<TaskItem?> Load(CreateAttachmentCommand cmd, CancellationToken ct)
        => await taskRepository.FirstOrDefaultAsync(new TaskByIdSpec(cmd.TaskId), ct);

    public async Task CheckAsync(
        CreateAttachmentCommand cmd, ValidationContext<CreateAttachmentCommand> context, CancellationToken ct)
    {
        var task = await Load(cmd, ct);
        if (task is null)
            context.AddFailure(nameof(cmd.TaskId), "Task does not exist.");
    }
}
