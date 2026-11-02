// Source: skill vp-postgres-in-dotnet

namespace Postgres.Abstractions;

/// <summary>
/// A unique key already taken: reported for a write that breaks a unique index,
/// and the service over the repository tells the caller what it means.
/// </summary>
public sealed class DuplicateKeyException(string message, Exception? storeError = null) : Exception(message, storeError);

/// <summary>A foreign key broken: the write points to a row that is not there.</summary>
public sealed class MissingReferenceException(string message, Exception? storeError = null) : Exception(message, storeError);

/// <summary>
/// The row is gone, or its version moved since it was read: the write changed
/// nothing, and the whole operation may be retried from the read.
/// </summary>
public sealed class RowChangedException(string message, Exception? storeError = null) : Exception(message, storeError);

/// <summary>
/// The database did not answer, or answered with a failure a retry may cure: no
/// connection, a connection lost, the server shutting down, a deadlock, a
/// serialization failure. Every other database failure is a defect of this
/// application and stays an exception with no kind.
/// </summary>
public sealed class StoreUnavailableException(string message, Exception? storeError = null) : Exception(message, storeError);
