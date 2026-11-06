# dotnet entities — status

The branch `arch-db-entities` collects the variation points of the new approach to the database; each stage reaches it by its own PR. It starts from `arch-new-vp` after #293 (shared scenarios per folder of `conformance/`). The row version of #292 is set aside: it is rebuilt over this approach later.

| Stage | State |
|---|---|
| 1 dotnet entities: the in-memory base (`ItemList`, `IRepository<T>`, `MemoryRepository<T>`), `vp-postgres-in-dotnet`, `vp-unit-of-work-in-dotnet`, the spec's two forms, `app-layers-in-dotnet` | done: `make test` green on PostgreSQL 17.5, audited, every check of the repository passes |
| next | `vp-row-version` over this approach (where the version of an aggregate's child lives: owner's decision), `vp-external-id`, `vp-row-lock`, `vp-idempotency` |
