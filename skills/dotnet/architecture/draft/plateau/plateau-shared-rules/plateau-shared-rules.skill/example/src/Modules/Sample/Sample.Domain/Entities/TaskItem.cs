using FluentValidation;
using FluentValidation.Results;
using Sample.Domain.Rules;
using Sample.Domain.ValueObjects;
using Sample.Interfaces.ValueObjects;
using Shared.Concurrency;
using Shared.Exceptions;
using Shared.Timestamps;

namespace Sample.Domain.Entities;

public class TaskItem : IVersioned, ICreationInfoModel, IUpdateInfoModel
{
    public int Id { get; internal set; }
    public string Title { get; private set; } = string.Empty;
    public int AssigneeId { get; internal set; }
    public Email AssigneeEmail { get; internal set; } = null!;
    public DateTimeOffset? StartDateTime { get; private set; }
    public DateTimeOffset? DueDateTime { get; private set; }
    public uint Version { get; internal set; }

    public DateTimeOffset UserCreatedDateTime { get; private set; }
    public DateTimeOffset ServerCreatedDateTime { get; internal set; }
    public DateTimeOffset UserUpdatedDateTime { get; private set; }
    public DateTimeOffset ServerUpdatedDateTime { get; internal set; }

    public TaskItem(int assigneeId, Email assigneeEmail)
    {
        AssigneeId = assigneeId;
        AssigneeEmail = assigneeEmail;
    }

    public void SetCreationInfo(DateTimeOffset userCreatedDateTime) => UserCreatedDateTime = userCreatedDateTime;
    public void SetUpdateInfo(DateTimeOffset userUpdatedDateTime) => UserUpdatedDateTime = userUpdatedDateTime;

    public void UpdateTitle(string title)
    {
        var result = new SoftTitle(title).Check();
        var blocking = result.Errors.FirstOrDefault(e => e.Severity == Severity.Error);
        if (blocking is not null)
            throw new DomainException(blocking.ErrorCode, blocking.ErrorMessage);

        Title = title;
    }

    public void UpdateSchedule(DateTimeOffset? startDateTime, DateTimeOffset? dueDateTime)
    {
        var result = new SoftSchedule(startDateTime, dueDateTime).Check();
        var blocking = result.Errors.FirstOrDefault(e => e.Severity == Severity.Error);
        if (blocking is not null)
            throw new DomainException(blocking.ErrorCode, blocking.ErrorMessage);

        StartDateTime = startDateTime;
        DueDateTime = dueDateTime;
    }
}
