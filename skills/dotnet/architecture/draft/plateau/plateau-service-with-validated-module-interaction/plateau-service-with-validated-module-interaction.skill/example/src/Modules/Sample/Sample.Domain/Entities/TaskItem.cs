using Sample.Domain.ValueObjects;
using Shared.Exceptions;

namespace Sample.Domain.Entities;

public class TaskItem
{
    public int Id { get; internal set; }
    public string Title { get; internal set; } = string.Empty;
    public int AssigneeId { get; internal set; }
    public Email AssigneeEmail { get; internal set; } = null!;
    public uint Version { get; internal set; }

    public TaskItem(int assigneeId, Email assigneeEmail)
    {
        AssigneeId = assigneeId;
        AssigneeEmail = assigneeEmail;
    }

    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Sample.TaskItem.TitleRequired", "Title must not be empty.");

        if (title.Length > 200)
            throw new DomainException("Sample.TaskItem.TitleTooLong", "Title must not exceed 200 characters.");

        Title = title;
    }
}
