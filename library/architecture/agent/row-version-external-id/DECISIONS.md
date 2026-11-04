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
