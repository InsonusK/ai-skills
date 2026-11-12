# Glossary

The concepts of the health variation point, explained for a reader new to them.

| Term | Page | Covers |
| --- | --- | --- |
| Health endpoints | [health-endpoints.md](./health-endpoints.md) | `/health/live`, `/health/ready`, `/health`: who calls each, what it checks, why three |
| Readiness check | [readiness-check.md](./readiness-check.md) | `management.Check`: how a dependency makes `/health/ready` answer 503 |
| Entry runner | [entry-runner.md](library/architecture/go/base-plateau-in-go.skill/glossary/entry-runner.md) (base plateau) | `entry.RunAll`: how the management entry point runs beside the others |
