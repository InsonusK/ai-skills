using Ardalis.Result;
using Shared;
using Shared.Concurrency;
using Shared.Timestamps;

namespace Sample.Interfaces.Commands;

public record UpdateTaskCommand(
    int TaskId,
    string Title,
    DateTimeOffset ActionTimeStamp,
    IReadOnlyDictionary<string, uint> Versions
) : ICommand<Result>, IHasVersions, ICommandWithTimestamp
{
    public IReadOnlyDictionary<string, int> EntityIds => new Dictionary<string, int> { ["Task"] = TaskId }.AsReadOnly();
}
