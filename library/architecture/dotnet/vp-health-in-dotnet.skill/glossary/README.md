# Glossary

The concepts of the health variation point, explained for a reader new to them.

| Term | Page | Covers |
| --- | --- | --- |
| Health endpoints | [health-endpoints.md](./health-endpoints.md) | `/health/live`, `/health/ready`, `/health`: who calls each, what it checks, why three |
| Readiness check | [readiness-check.md](./readiness-check.md) | `ReadinessCheck`: how a dependency's `IHealthCheck` makes `/health/ready` answer 503 |
| Entry runner | [entry-runner.md](library/architecture/dotnet/base-plateau-in-dotnet.skill/glossary/entry-runner.md) (base plateau) | `EntryRunner.RunAllAsync`: how the management entry point runs beside the others |
