// Source: skill vp-postgres-in-dotnet

namespace Postgres.Abstractions;

/// <summary>
/// A unique key already taken: a store reports it for an insert that breaks a
/// unique index, and the service over the store tells the caller what it means.
/// </summary>
public sealed class DuplicateKeyException(string message, Exception? storeError = null) : Exception(message, storeError);

/// <summary>
/// The database did not answer: no connection, a connection lost, the server
/// shutting down. A retry may help; every other database failure is a defect of
/// this application and stays an exception with no kind.
/// </summary>
public sealed class StoreUnavailableException(string message, Exception? storeError = null) : Exception(message, storeError);
