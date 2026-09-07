using Sample.Domain.ValueObjects;
using Shared.Concurrency;
using Shared.Exceptions;
using Shared.Timestamps;

namespace Sample.Domain.Entities;

// VP1: guarded state transitions, DomainException on an invariant violation.
// VP5: IVersioned + uint Version (bumped by AppDbContext; maps to xmin in production).
// VP7: creation + update timestamps (server times set by AppDbContext, user times by the handler).
public class TodoItem : IVersioned, ICreationInfoModel, IUpdateInfoModel
{
    public int Id { get; internal set; }
    public ItemTitle Title { get; internal set; } = null!;
    public bool IsDone { get; internal set; }
    public uint Version { get; internal set; }

    public DateTimeOffset ServerCreatedDateTime { get; internal set; }
    public DateTimeOffset UserCreatedDateTime { get; internal set; }
    public DateTimeOffset ServerUpdatedDateTime { get; internal set; }
    public DateTimeOffset UserUpdatedDateTime { get; internal set; }

    DateTimeOffset ICreationInfoModel.ServerCreatedDateTime { get => ServerCreatedDateTime; set => ServerCreatedDateTime = value; }
    DateTimeOffset ICreationInfoModel.UserCreatedDateTime { get => UserCreatedDateTime; set => UserCreatedDateTime = value; }
    DateTimeOffset IUpdateInfoModel.ServerUpdatedDateTime { get => ServerUpdatedDateTime; set => ServerUpdatedDateTime = value; }
    DateTimeOffset IUpdateInfoModel.UserUpdatedDateTime { get => UserUpdatedDateTime; set => UserUpdatedDateTime = value; }

    private TodoItem() { } // EF materialization

    public static TodoItem Create(ItemTitle title) => new() { Title = title };

    public void Rename(ItemTitle newTitle)
    {
        if (IsDone)
            throw new DomainException("Sample.TodoItem.RenameCompleted", "A completed item cannot be renamed.");

        Title = newTitle;
    }

    public void Complete()
    {
        if (IsDone)
            throw new DomainException("Sample.TodoItem.AlreadyDone", "The item is already completed.");

        IsDone = true;
    }

    // VP7: the handler copies the command's ActionTimeStamp onto the entity; server times are set on save.
    public void RecordCreatedByUser(DateTimeOffset at)
    {
        UserCreatedDateTime = at;
        UserUpdatedDateTime = at;
    }

    public void RecordUpdatedByUser(DateTimeOffset at) => UserUpdatedDateTime = at;
}
