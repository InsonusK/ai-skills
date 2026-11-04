# One shape of error — status

The plan by waves; each wave passes `check.sh`, gets a fresh-eyes audit, and is one commit.

| Wave | What | State |
|---|---|---|
| 0 | `INVARIANTS.md`, `DECISIONS.md`, `check.sh`; the sample: Go's `base-plateau-in-go` example and `error-kinds-in-go` | done; `INVARIANTS.md` agreed by the owner |
| 1 | Go: every other example in scope (shared copies, then each VP's own files), the templates, the Go skills | done: 11 examples pass `go vet` and `go test` |
| 2 | Python: `shared/errors`, every example in scope, the templates, `error-kinds-in-python`, the Python skills | done: 11 examples pass `make test` and `make lint` |
| 3 | dotnet: `cause` in `JsonLogLine`, every copy (the DB examples' copies too), `error-kinds-in-dotnet`, the dotnet skills | done: 12 examples pass `dotnet test` (the two DB ones on PostgreSQL 17.5) |
| 4 | `error-kinds` (core), the catalog's README, the glossaries and ADRs that list the log fields | done; `check.sh --ground` passes |

Examples in scope, per stack: `base-plateau`, `base-plateau-extension`, `vp-cli-api` (Go, Python), `vp-client-retry`, `vp-grpc-api`, `vp-grpc-client`, `vp-health`, `vp-http-api` (`examples`, `examples-proto`), `vp-http-client`, `vp-logging`.

Found on the way, not part of this migration: `AccessConfig.feature` of `base-plateau-extension-in-dotnet` fails about one run in six - its scenarios set and read environment variables while xUnit runs them in parallel (`AccessConfigSteps.ThenTheTokenShouldName` sees no tokens).

Left for the DB branches (#281-#284), once merged: `vp-postgres-in-*` and `vp-unit-of-work-in-*` build their errors with the new shape (Go `errorkind.Wrap` for a store's failure, Python `shared.errors`); the dotnet DB examples already carry the new shared files.
