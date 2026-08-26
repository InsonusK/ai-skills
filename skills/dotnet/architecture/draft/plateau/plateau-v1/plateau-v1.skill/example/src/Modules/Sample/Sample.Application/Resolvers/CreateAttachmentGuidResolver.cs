using Ardalis.Result;
using Sample.Application.Specifications;
using Sample.Domain.Entities;
using Sample.Interfaces.Commands;
using Shared.Guid;
using Shared.Repositories;
using Shared.Results;

namespace Sample.Application.Resolvers;

public sealed class CreateAttachmentGuidResolver(IReadRepository<Attachment> repository)
    : IGuidResolver<Result<CreateAttachmentResult>>
{
    public async Task<Result<CreateAttachmentResult>?> ResolveAsync(System.Guid guid, CancellationToken ct)
    {
        var existing = await repository.FirstOrDefaultAsync(new AttachmentByGuidSpec(guid), ct);
        return existing is null ? null : ConflictResult<CreateAttachmentResult>.For(new CreateAttachmentResult());
    }
}
