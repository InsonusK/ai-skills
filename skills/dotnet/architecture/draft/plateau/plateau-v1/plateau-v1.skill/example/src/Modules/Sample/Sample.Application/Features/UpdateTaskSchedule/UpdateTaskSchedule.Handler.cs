using Ardalis.Result;
using MediatR;
using Sample.Application.Specifications;
using Sample.Domain.Entities;
using Sample.Interfaces.Commands;
using Shared.Repositories;

namespace Sample.Application.Features.UpdateTaskSchedule;

public class UpdateTaskScheduleHandler(IRepository<TaskItem> repository) : IRequestHandler<UpdateTaskScheduleCommand, Result>
{
    public async Task<Result> Handle(UpdateTaskScheduleCommand command, CancellationToken ct)
    {
        var task = await repository.FirstOrDefaultAsync(new TaskByIdSpec(command.TaskId), ct);
        if (task is null)
            return Result.NotFound();

        task.UpdateSchedule(command.StartDateTime, command.DueDateTime);
        task.SetUpdateInfo(command.ActionTimeStamp);

        await repository.UpdateAsync(task, ct);

        return Result.Success();
    }
}
