namespace Shared.Timestamps;

// A create/update command for a timestamped entity carries the user's action time.
public interface ICommandWithTimestamp
{
    DateTimeOffset ActionTimeStamp { get; }
}
