namespace Shared.Timestamps;

public interface ICreationInfoModel : ICreationInfoModelReadOnly
{
    new DateTimeOffset ServerCreatedDateTime { get; set; }
    new DateTimeOffset UserCreatedDateTime { get; set; }
}
