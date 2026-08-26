using Shared.Exceptions;
using Shared.Guid;
using Shared.Timestamps;

namespace Sample.Domain.Entities;

public class Attachment : IHasGuid, ICreationInfoModel
{
    public int Id { get; internal set; }
    public System.Guid Guid { get; internal set; }
    public int TaskId { get; internal set; }
    public string FileName { get; internal set; } = string.Empty;

    public DateTimeOffset UserCreatedDateTime { get; private set; }
    public DateTimeOffset ServerCreatedDateTime { get; internal set; }

    public Attachment(System.Guid guid, int taskId, string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainException("Sample.Attachment.FileNameRequired", "File name must not be empty.");

        if (fileName.Length > 500)
            throw new DomainException("Sample.Attachment.FileNameTooLong", "File name must not exceed 500 characters.");

        Guid = guid;
        TaskId = taskId;
        FileName = fileName;
    }

    public void SetCreationInfo(DateTimeOffset userCreatedDateTime) => UserCreatedDateTime = userCreatedDateTime;
}
