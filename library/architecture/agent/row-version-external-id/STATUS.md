# Row version and external id — status

| Wave | What | State |
|---|---|---|
| 0 | spec of both, `conformance/`, `vp-postgres`'s classification | done; specs agreed by the owner |
| 1 | `vp-row-version-in-{go,python,dotnet}` | done in every stack: the base check in vp-postgres-in-{go,python} (dotnet's EF Core has it), the examples with CompleteItem; the shared scenarios pass and catch every mutation tried |
| 2 | `vp-external-id-in-{go,python,dotnet}` | — |
| later | `vp-idempotency` | — |
