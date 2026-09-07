namespace Shared.Timestamps;

public interface ICreationInfoModelReadOnly
{
    DateTimeOffset ServerCreatedDateTime { get; }
    DateTimeOffset UserCreatedDateTime { get; }
}
