using Shared.Errors;
using Todo.Domain.Entities;

namespace Todo.Domain.Errors;

/// <summary>
/// The errors of the module's rules, one factory method each, each carrying its
/// kind (skill error-kinds). A method, not a shared instance: an exception
/// records its stack when thrown, so every throw needs its own.
/// </summary>
public static class TodoErrors
{
    public static ErrorKindException EmptyTitle() =>
        new(ErrorKind.Invalid, "item title is empty");

    public static ErrorKindException TitleTooLong() =>
        new(ErrorKind.Invalid, $"item title is longer than {Item.MaxTitleLength} characters");

    // The input is valid; the item's state forbids it.
    public static ErrorKindException AlreadyDone() =>
        new(ErrorKind.FailedPrecondition, "item is already done");

    public static ErrorKindException NoItems() =>
        new(ErrorKind.Invalid, "no items to plan");

    public static ErrorKindException DuplicateTitle() =>
        new(ErrorKind.Invalid, "item titles repeat");

    // Raised by the service over the store, from the store's technical outcome.
    public static ErrorKindException ItemAlreadyInList(string listId, string title, Exception? storeError = null) =>
        new(ErrorKind.Conflict, $"list \"{listId}\" already has an item \"{title}\"", storeError);

    public static ErrorKindException NoList(string listId) =>
        new(ErrorKind.NotFound, $"no list \"{listId}\"");

    public static ErrorKindException ListsUnavailable(Exception storeError) =>
        new(ErrorKind.Unavailable, "the item lists are unavailable", storeError);
}
