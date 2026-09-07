namespace Shared.Timestamps;

public interface IUpdateInfoModelReadOnly
{
    DateTimeOffset ServerUpdatedDateTime { get; }
    DateTimeOffset UserUpdatedDateTime { get; }
}
