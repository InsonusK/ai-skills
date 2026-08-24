namespace Shared.Timestamps;

public interface ICreationInfoModelReadOnly
{
    DateTimeOffset UserCreatedDateTime { get; }
    DateTimeOffset ServerCreatedDateTime { get; }
}

public interface ICreationInfoModel : ICreationInfoModelReadOnly
{
    void SetCreationInfo(DateTimeOffset userCreatedDateTime);
}

public interface IUpdateInfoModelReadOnly
{
    DateTimeOffset UserUpdatedDateTime { get; }
    DateTimeOffset ServerUpdatedDateTime { get; }
}

public interface IUpdateInfoModel : IUpdateInfoModelReadOnly
{
    void SetUpdateInfo(DateTimeOffset userUpdatedDateTime);
}

public interface ICommandWithTimestamp
{
    DateTimeOffset ActionTimeStamp { get; }
}
