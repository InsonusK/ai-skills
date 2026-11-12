// Source: skill vp-unit-of-work-in-dotnet

using Shared.Errors;

namespace Shared.UnitOfWork;

/// <summary>The errors of the unit of work: no service stands over a commit to tell its outcome.</summary>
public static class UnitOfWorkErrors
{
    public static ErrorKindException Unavailable(Exception storeError) =>
        new(ErrorKind.Unavailable, "the changes could not be committed: the database does not answer", storeError);
}
