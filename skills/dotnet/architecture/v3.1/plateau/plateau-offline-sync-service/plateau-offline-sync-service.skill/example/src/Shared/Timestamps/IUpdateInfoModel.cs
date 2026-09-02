namespace Shared.Timestamps;

public interface IUpdateInfoModel : IUpdateInfoModelReadOnly
{
    new DateTimeOffset ServerUpdatedDateTime { get; set; }
    new DateTimeOffset UserUpdatedDateTime { get; set; }
}
