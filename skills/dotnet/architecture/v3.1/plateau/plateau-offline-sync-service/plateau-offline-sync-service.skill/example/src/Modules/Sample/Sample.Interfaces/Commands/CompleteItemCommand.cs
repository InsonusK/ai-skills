using Ardalis.Result;
using Shared.Concurrency;
using Shared.MediatR;
using Shared.Timestamps;

namespace Sample.Interfaces.Commands;

public record CompleteItemCommand(int ItemId, DateTimeOffset ActionTimeStamp, uint ExpectedVersion)
    : ICommand<Result>, ICommandWithTimestamp, IHasVersions
{
    public IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>> Versions =>
        new Dictionary<string, IReadOnlyDictionary<int, uint>>
        {
            ["TodoItem"] = new Dictionary<int, uint> { [ItemId] = ExpectedVersion },
        };
}
