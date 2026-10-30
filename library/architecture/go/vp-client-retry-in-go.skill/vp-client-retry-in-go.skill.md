---
name: vp-client-retry-in-go
description: The client retry variation point in Go — internal/infrastructure/client/middleware/retry copied from templates (LoadConfig reading {API}_RETRY_ATTEMPTS and {API}_RETRY_BACKOFF, Middleware repeating a Retryable call on outcome.ErrUnavailable with doubled, jittered waits within the caller's deadline and a WARN record per repeat), added to the client middleware of an API in main.go
whenToUse: when a Go application with a client of another service's API (vp-http-client-in-go, vp-grpc-client-in-go) must repeat calls the API did not answer, when you mark an operation retryable, or when you review a Go client retry
updated: 20261030
tags:
  - stack/go
  - concern/architecture
  - app-type/service
  - app-type/cli
---

# Goal
- **The client retry variation point in Go** - A Go application that keeps every property of [vp-client-retry](library/architecture/spec/vp-client-retry.skill/vp-client-retry.skill.md); this skill details its implementation for Go and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds the middleware and changes only the files [attach.allow](./attach.allow) names, which the repository's attach check verifies against [vp-http-client-in-go](library/architecture/go/vp-http-client-in-go.skill/vp-http-client-in-go.skill.md)'s example; over [vp-grpc-client-in-go](library/architecture/go/vp-grpc-client-in-go.skill/vp-grpc-client-in-go.skill.md) it is attached the same way.

# Core Principle
- **A middleware of client/core/pipeline** - `retry.Middleware` is a `pipeline.Middleware`: it sees `pipeline.Call` (API, operation, `Retryable`) and calls `next`, the attempt, once or more; the client runs it around its attempts, so it is the same over `httpclient` and `grpcclient`.

```
internal/infrastructure/client/
├── core/pipeline/           of the client: Call, Attempt, Middleware, Run
├── middleware/retry/        copied from templates, never edited
│   ├── config.go            LoadConfig(prefix): {API}_RETRY_ATTEMPTS (3), {API}_RETRY_BACKOFF (100ms)
│   ├── retry.go             Middleware(cfg, logger): Retryable + ErrUnavailable → wait, attempt again
│   └── features/ test/      the scenarios, with attempts that end as each scenario lists
└── {api}/{api}.go           Operations / Methods: Retryable where a repeat does what one call does
```

# Rule

## MUST

### Copy the middleware from templates
Fill and copy [config.go](./templates/internal/infrastructure/client/middleware/retry/config.go) and [retry.go](./templates/internal/infrastructure/client/middleware/retry/retry.go) to `internal/infrastructure/client/middleware/retry/`, with the example's `features/` and `test/`, replacing `{module-path}` with the module path from `go.mod`.
- Risk: a retyped retry repeats unmarked calls or statuses, or waits without jitter.
- Fix: copy the files; after filling, searching them for `{module-path}` returns nothing; `make test` passes.

### List the retry in the client middleware of the API
Load the settings with `retry.LoadConfig({api}.Prefix)` in `// [client middleware]` of `cmd/{app}/main.go`, stopping the start with its error, and add `retry.Middleware(cfg, logger)` to the list passed to `{api}.New`.

```go
// [client middleware]
todoAPIRetry, err := retry.LoadConfig(todoapi.Prefix)
if err != nil {
	logger.Error("startup failed", "error", err)
	return 1
}
clientMiddlewares := []clientpipeline.Middleware{
	retry.Middleware(todoAPIRetry, logger),
}
```

- Violation: the retry wrapped around a client method or a service; one retry config shared by two APIs.
- Risk: a repeat outside the client repeats the mapping and misses the mark; two APIs cannot be tuned apart.
- Fix: one list per API, built before its client; a middleware added after the retry runs once per attempt.

### Mark the retryable operations in the client
Set `Retryable: true` on each `httpclient.Operation` or `grpcclient.Method` of the client's list whose repeat does what one call does, with a comment saying why, and hold the mark in the contract scenario's table.
- Risk: an unmarked operation is never repeated, a wrongly marked one is done twice.
- Fix: the example's `todoapi.Operations` marks `POST /v1/items/plan`, which stores nothing.

# Check list
- [ ] `internal/infrastructure/client/middleware/retry/{config,retry}.go` are filled copies of the templates.
- [ ] `main.go` loads the retry settings of each API and lists the middleware in its client middleware.
- [ ] Only operations whose repeat does what one call does are `Retryable`; the contract scenario holds the marks.
- [ ] `make test` passes; `bash test/check-vp-attach.sh` in this repository reports no file changed beyond [attach.allow](./attach.allow).

# Example
- [examples/](./examples/README.md) — the HTTP client example with the retry attached: `TODO_API_HOST=localhost TODO_API_HTTP_PORT=1 make run` shows three attempts and two "api client call retried" warnings before the startup command fails unavailable.
