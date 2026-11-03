# Readiness check

A **readiness check** is one dependency the service needs before it can serve a request — a database, its message broker — registered with `AddHealthChecks()` as a name and an ASP.NET `IHealthCheck`. `/health/ready` runs every check on each call and answers 503, naming the failed ones, while any of them reports `Unhealthy` or throws.

## Why it exists

The management entry point knows no dependency: which ones a service has depends on the variation points attached to it. So each service variation point registers the check of the dependency it adds in the framework's registry, which belongs to no variation point, and the management entry point reads it. `Api/Management` is never edited, and the variation point builds without it.

`IHealthCheck` is ASP.NET's own contract, so the ready-made checks of the ecosystem — `AspNetCore.HealthChecks.NpgSql`, `...Redis`, `...RabbitMQ`, `...Kafka` — are used as they are; a check of the application's own is one small class.

## How it works

1. A service variation point's own registration calls `AddHealthChecks()` in `// [services]`, in the application's container.
2. In `// [entries]`, `AddManagement()` adds the entry; when the entry starts, it builds each registered check once.
3. On `GET /health/ready`, the server calls every check with the request's token; each failure is logged as "readiness check failed" with its name.

```csharp
// in AddPostgres, brought by the database variation point
services.AddHealthChecks().AddNpgSql(config.Dsn, name: "database");

// [entries]
services.AddManagement();
```

| Result of the check | `/health/ready` |
|---|---|
| `Healthy` | passes |
| `Degraded` | passes — the service still takes traffic, slower or with less |
| `Unhealthy`, or an exception | fails: 503, the name in `failed`, a warning in the log |

## What a check must not do

- **Send a command** — a probe that changes data or runs business logic runs it every few seconds on every replica.
- **Be slow** — the probe has a timeout; a check that takes seconds marks a healthy replica unready.
- **Check something the service can live without** — a dependency the service degrades around (a cache) reports `Degraded`, not `Unhealthy`.

## Related concepts

- [Health endpoints](./health-endpoints.md) — which endpoint runs the checks, and why liveness never does.
- [Entry runner](library/architecture/dotnet/base-plateau-in-dotnet.skill/glossary/entry-runner.md) — runs the management entry point beside the others.

## Sources

- Health checks in ASP.NET Core: https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks
- AspNetCore.Diagnostics.HealthChecks: https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
