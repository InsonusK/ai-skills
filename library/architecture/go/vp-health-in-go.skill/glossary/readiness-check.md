# Readiness check

A **readiness check** (`management.Check`) is one dependency the service needs before it can serve a request — a database, another service — given to the management entry point as a name and a function. `/health/ready` runs every check on each call and answers 503, naming the failed ones, while any of them returns an error.

## Why it exists

The management entry point knows no dependency: which ones a service has depends on the variation points attached to it. So each service variation point brings the check of the dependency it adds, and the composition root hands the checks over. The management package is never edited.

## How it works

1. A service variation point builds its infrastructure in `// [services]` of `main.go` (a database pool, an HTTP client).
2. In `// [entries]`, the composition root passes one `management.Check` per dependency to `management.Entry`.
3. On `GET /health/ready`, the server calls every `Check` with the request's context; each failure is logged with its name.

```go
// [services]
database, err := postgres.Open(ctx, postgres.LoadConfig())   // brought by the DB variation point

// [entries]
management.Entry(logger,
	management.Check{Name: "database", Check: database.Ping},
),
```

| Part | Role |
|---|---|
| `Name` | how a failure is reported in the body and the log: `"database"` |
| `Check func(ctx) error` | `nil` while the dependency can be used; a cheap call — a ping, not a query of business data |

## What a check must not do

- **Call a command** — a probe that changes data or runs business logic runs it every few seconds on every replica.
- **Be slow** — the probe has a timeout; a check that takes seconds marks a healthy replica unready.
- **Check something the service can live without** — a dependency the service degrades around (a cache) keeps it ready.

## Related concepts

- [Health endpoints](./health-endpoints.md) — which endpoint runs the checks, and why liveness never does.
- [Entry runner](library/architecture/go/base-plateau-in-go.skill/glossary/entry-runner.md) — runs the management entry point beside the others.
