using Ardalis.Result;
using MediatR;
using Sample.Domain.Entities;
using Sample.Interfaces.Commands;
using Sample.Application.Specifications;
using Shared.Repositories;

namespace Sample.Application.Features.UpdateTask;

public class UpdateTaskHandler(IRepository<TaskItem> repository) : IRequestHandler<UpdateTaskCommand, Result>
{
    public async Task<Result> Handle(UpdateTaskCommand command, CancellationToken cancellationToken)
    {
        var task = await repository.FirstOrDefaultAsync(new TaskByIdSpec(command.TaskId), cancellationToken);
        if (task is null)
            return Result.NotFound();

        task.UpdateTitle(command.Title);
        task.SetUpdateInfo(command.ActionTimeStamp);

        await repository.UpdateAsync(task, cancellationToken);

        return Result.Success();
    }
}
