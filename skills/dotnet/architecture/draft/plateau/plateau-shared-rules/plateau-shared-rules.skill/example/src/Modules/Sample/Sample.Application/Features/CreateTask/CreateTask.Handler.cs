using Ardalis.Result;
using MediatR;
using Sample.Domain.Entities;
using Sample.Domain.ValueObjects;
using Sample.Interfaces.Commands;

namespace Sample.Application.Features.CreateTask;

public class CreateTaskHandler : IRequestHandler<CreateTaskCommand, Result<CreateTaskResult>>
{
    public Task<Result<CreateTaskResult>> Handle(CreateTaskCommand command, CancellationToken cancellationToken)
    {
        var task = new TaskItem(command.AssigneeId, new Email(command.AssigneeEmail.Value));

        task.UpdateTitle(command.Title.Value);
        task.UpdateSchedule(command.StartDateTime, command.DueDateTime);

        return Task.FromResult(Result.Created(new CreateTaskResult(task.Id)));
    }
}
