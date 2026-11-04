# Row version and external id — decisions

One line per choice; ⚠️ marks a fork with real trade-offs, decided by the owner.

- ⚠️ 2026-11-04, owner: one spec pass for both variation points and where they meet; the code of `vp-row-version` first, then `vp-external-id`, each its own PR.
- ⚠️ 2026-11-04, owner: an entity created in an external system has a UUID v7 as its PK; an entity the service creates keeps a `bigint` identity. Chosen over `bigint` PK + unique guid after measuring both on PostgreSQL 17.5 (`INVARIANTS.md` §3).
- ⚠️ 2026-11-04, owner: the agent warns the user that a UUID v7 holds the creation time when that time may be secret.
- ⚠️ 2026-11-04, owner: a repeated create is told apart by an idempotency key in a header, kept in a table of its own: a variation point `vp-idempotency`, after this pair; without a matching key, a taken id is `conflict`.
- ⚠️ 2026-11-04, owner: the base repository of `vp-postgres` compares the version when a row declares one, in every stack (dotnet's EF Core already does).
- 2026-11-02, owner: the version is the engine's `xmin` (ADR `version-kept-by-the-engine`).
- 2026-11-02, agent: only the write compares the version (ADR `one-check-in-the-write`).
- 2026-11-04, agent: the version is an opaque `string` in every contract.
- 2026-11-04, agent: a versioned row answers its new version on create and on update.
- 2026-11-04, agent: the entry point refuses an id that is no UUID v7 as `invalid`; no check of its time.
- 2026-11-04, agent: Go declares the version as a field `postgres.Version` tagged `column:xmin;->;-:migration`; the base reads every column by name (`QueryFields`), since `*` leaves `xmin` out, and refuses (`ErrRefused`) a write of a versioned row with no version, a defect of a caller that skipped the read.
- 2026-11-04, agent: the example's changing table is `list_items`: `CompleteItem` (proto 1.2.0, `POST /v1/lists/{listId}/items/{itemId}/complete`) reads the item, applies the entity's rule, and saves under the caller's version; every item answered carries `id` and `version`. Go's layers let no command import the service, so the id and the version go up in `entity.KeptItem`.
- 2026-11-04, agent: a row that never changes is marked by a method `NeverChanges()` (Go `tables.NeverChanges`); the shared scenario checks every mapped row has the version or the mark.
- 2026-11-04, agent: the shared scenarios gain "A delete with a version a caller kept and nobody moved deletes the row": the stale delete alone left the successful one unchecked.
- 2026-11-04, agent: Python declares the version as `version_column()`: `xmin` of a type over `xid` (asyncpg sends it from an int), a system column with `FetchedValue`, read as text; Alembic's comparison leaves system columns out; the selector's update saves a versioned row by the repository's `save`, every picked row detached first so no autoflush writes one without its version.
- 2026-11-04, agent: dotnet keeps a `uint Version` configured `IsRowVersion()` (Npgsql's `xmin`); the migration EF Core writes for it has no DDL. The service builds a new row with the caller's version, so EF Core compares that one; a version that is no number matches no row and is a concurrent change. The service scenarios fake the store (`InMemoryRepository`); the comparison itself is proven by the shared scenarios and by `Persistence.feature` over the whole application.
