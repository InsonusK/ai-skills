# Readiness check

A **readiness check** (`ReadinessCheck(Name, IHealthCheck)`) is one dependency the service needs before it can serve a request — a database, another service — given to the management entry point as a name and an ASP.NET `IHealthCheck`. `/health/ready` runs every check on each call and answers 503, naming the failed ones, while any of them reports `Unhealthy` or throws.

## Why it exists

The management entry point knows no dependency: which ones a service has depends on the variation points attached to it. So each service variation point brings the check of the dependency it adds, and the composition root hands the checks over. `Api/Management` is never edited.

`IHealthCheck` is ASP.NET's own contract, so the ready-made checks of the ecosystem — `AspNetCore.HealthChecks.NpgSql`, `...Redis`, `...RabbitMQ`, `...Kafka` — are used as they are; a check of the application's own is one small class.

## How it works

1. A service variation point builds its infrastructure in `// [services]` of `Program.cs` (a connection string, an HTTP client).
2. In `// [entries]`, the composition root passes one `ReadinessCheck` per dependency to `AddManagement`.
3. On `GET /health/ready`, the server calls every check with the request's token; each failure is logged as "readiness check failed" with its name.

```csharp
// [entries]
services.AddManagement(
    new ReadinessCheck("database", new NpgSqlHealthCheck(new NpgSqlHealthCheckOptions(connectionString))));
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
