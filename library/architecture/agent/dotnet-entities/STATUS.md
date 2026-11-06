# dotnet entities — status

| Wave | State |
|---|---|
| 1a in-memory base: `ItemList` aggregate, `IRepository<T>`, `MemoryRepository<T>` | done: `make test` green |
| 1b `vp-postgres-in-dotnet`, spec, `app-layers-in-dotnet` | done: `make test` green on PostgreSQL 17.5; 4 mutations caught; audit pending |
| 2 `vp-unit-of-work-in-dotnet`, `vp-row-version-in-dotnet` | — : their `check-vp-attach` fails until then |
