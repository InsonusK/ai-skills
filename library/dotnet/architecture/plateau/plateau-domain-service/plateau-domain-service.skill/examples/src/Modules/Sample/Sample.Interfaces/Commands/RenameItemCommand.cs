using Ardalis.Result;
using Sample.Interfaces.ValueObjects;
using Shared.Concurrency;
using Shared.MediatR;
using Shared.Timestamps;

namespace Sample.Interfaces.Commands;

// Order: business fields (ItemId, NewTitle), ActionTimeStamp (VP7), version token (VP5).
public record RenameItemCommand(int ItemId, SoftItemTitle NewTitle, DateTimeOffset ActionTimeStamp, uint ExpectedVersion)
    : ICommand<Result>, ICommandWithTimestamp, IHasVersions
{
    public IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>> Versions =>
        new Dictionary<string, IReadOnlyDictionary<int, uint>>
        {
            ["TodoItem"] = new Dictionary<int, uint> { [ItemId] = ExpectedVersion },
        };
}
