using Ardalis.Result;
using MediatR;
using Sample.Domain.Entities;
using Sample.Domain.ValueObjects;
using Sample.Interfaces.Commands;
using Shared.Repositories;

namespace Sample.Application.Features.CreateTask;

public class CreateTaskHandler(IRepository<TaskItem> repository) : IRequestHandler<CreateTaskCommand, Result<CreateTaskResult>>
{
    public async Task<Result<CreateTaskResult>> Handle(CreateTaskCommand command, CancellationToken cancellationToken)
    {
        var task = new TaskItem(command.AssigneeId, new Email(command.AssigneeEmail.Value));
        task.UpdateTitle(command.Title.Value);
        task.UpdateSchedule(command.StartDateTime, command.DueDateTime);
        task.SetCreationInfo(command.ActionTimeStamp);

        await repository.AddAsync(task, cancellationToken);

        return Result.Created(new CreateTaskResult { Id = task.Id });
    }
}
