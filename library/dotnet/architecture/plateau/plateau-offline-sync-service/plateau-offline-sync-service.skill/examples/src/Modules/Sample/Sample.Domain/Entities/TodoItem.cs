using Sample.Domain.ValueObjects;
using Shared.Concurrency;
using Shared.Exceptions;
using Shared.Guid;
using Shared.Timestamps;

namespace Sample.Domain.Entities;

// Classification: External Mutable (VP5 x VP6) — a client generates the Guid offline, the
// server owns the concurrency Version, and the item is edited after creation.
//   VP1: guarded transitions + DomainException     VP5: IVersioned + uint Version
//   VP6: IHasGuid + Guid (set once)                VP7: creation + update timestamps
public class TodoItem : IVersioned, IHasGuid, ICreationInfoModel, IUpdateInfoModel
{
    public int Id { get; internal set; }
    public System.Guid Guid { get; internal set; }
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

    private TodoItem() { }

    // VP6: the Guid is a creation parameter, set once, never changed.
    public static TodoItem Create(System.Guid guid, ItemTitle title) => new() { Guid = guid, Title = title };

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

    public void RecordCreatedByUser(DateTimeOffset at) { UserCreatedDateTime = at; UserUpdatedDateTime = at; }
    public void RecordUpdatedByUser(DateTimeOffset at) => UserUpdatedDateTime = at;
}
