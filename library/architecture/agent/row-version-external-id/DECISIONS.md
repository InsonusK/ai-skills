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
- 2026-11-14, owner: finish the integration chain and row version on the current catalog, then reconcile its statuses and outstanding tasks.
- 2026-11-14, agent: #294 and its integration PR #300 are merged. Port the Go/Python work from #292 onto this base; preserve the newer dotnet entities/CLI implementation and use a successor PR instead of force-updating the obsolete branch.
- 2026-11-14, agent, R1 audit: optional version checks preserve unversioned writes. Python detaches all selected rows before its first versioned save to prevent autoflush bypass; templates and UoW copies carry the same fix. Caller-version boundary cases are explicitly exercised in the next sample waves.
- 2026-11-14, agent, metadata audit: replace legacy db/postgres with the canonical db/postgresql in shared sources and native features, then sync copies; classify legacy edge scenarios by their actual boundary/negative/error/concurrency intent (metadata-classification.json). No behavior/assertions changed.
- 2026-11-14, agent: the repository tag gate reads the canonical testing vocabulary and validates effective Scenario/Examples categories, Feature-only types and pytest registrations. It is wired into both shell and PowerShell validation. Fresh-eyes audit found an Examples type-tag hole, fixed and mutation-probed.
