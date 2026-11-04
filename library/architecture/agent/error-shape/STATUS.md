# One shape of error — status

The plan by waves; each wave passes `check.sh`, gets a fresh-eyes audit, and is one commit.

| Wave | What | State |
|---|---|---|
| 0 | `INVARIANTS.md`, `DECISIONS.md`, `check.sh`; the sample: Go's `base-plateau-in-go` example and `error-kinds-in-go` | done; `INVARIANTS.md` agreed by the owner |
| 1 | Go: every other example in scope (shared copies, then each VP's own files), the templates, the Go skills | done: 11 examples pass `go vet` and `go test` |
| 2 | Python: `shared/errors`, every example in scope, the templates, `error-kinds-in-python`, the Python skills | — |
| 3 | dotnet: `cause` in `JsonLogLine`, every copy (the DB examples' copies too), `error-kinds-in-dotnet`, the dotnet skills | — |
| 4 | `error-kinds` (core), the catalog's README, the glossaries and ADRs that list the log fields | — |

Examples in scope, per stack: `base-plateau`, `base-plateau-extension`, `vp-cli-api` (Go, Python), `vp-client-retry`, `vp-grpc-api`, `vp-grpc-client`, `vp-health`, `vp-http-api` (`examples`, `examples-proto`), `vp-http-client`, `vp-logging`.
