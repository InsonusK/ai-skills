using Shared.Errors;
using Todo.Domain.Errors;

namespace Todo.Domain.Tests.Steps;

/// <summary>The rule errors as the scenarios name them.</summary>
public static class RuleErrors
{
    private static readonly Dictionary<string, Func<ErrorKindException>> ByName = new()
    {
        ["empty title"] = TodoErrors.EmptyTitle,
        ["title too long"] = TodoErrors.TitleTooLong,
        ["already done"] = TodoErrors.AlreadyDone,
        ["no items"] = TodoErrors.NoItems,
        ["duplicate title"] = TodoErrors.DuplicateTitle,
    };

    /// <summary>The scenario's name of <paramref name="error"/>: the rule error with the same kind and message, or its message.</summary>
    public static string NameOf(ErrorKindException error) =>
        ByName.FirstOrDefault(rule => rule.Value() is var known && known.Kind == error.Kind && known.Message == error.Message).Key
        ?? error.Message;
}
