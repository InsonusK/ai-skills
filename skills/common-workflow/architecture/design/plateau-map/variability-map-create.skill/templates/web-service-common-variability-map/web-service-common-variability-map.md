# Web-service common Variability Map

The Variation Points every backend web-service catalog shares, whatever its stack. Each one is defined here once — question, Variants, Constraint, Realization depends on, and the concept behind them — and **inherited** by every bound stack map, which adds only its State, its narrowing, and its `Realized by`. Rules: [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md#common-variation-points|variability-map-create — Common Variation Points]]. Derived from, and kept consistent with, [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/templates/web-service-common-features/web-service-common-features|web-service-common-features]].

A VP enters this map only through [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md#how-to-admit-a-common-variation-point|admission]]: discussed with the owner and designed on every bound stack in the same change. The map starts empty and grows one VP at a time.

## Common Variation Points

| ID | VP | Variants | Constraint | Realization depends on |
| --- | --- | --- | --- | --- |
| VP-C001 | **PersistentStore** — where does the service keep data it must never lose? | None / PostgreSQL / SQLite | — | — |
| VP-C002 | **TransientStore** — where does the service keep data it can survive losing? | None / Redis / InMemory | — | — |

### VP-C001 PersistentStore
The system of record: data that survives restarts and redeploys and is never deliberately discarded. Business changes are written in transactions, which later VPs (TaskBox, Outbox) rely on to stay atomic with the change they report.
- **None** — the service owns no durable state (stateless, or delegates state to other services).
- **PostgreSQL** — a separate database server; fits any number of service instances.
- **SQLite** — an embedded database file; binds the service to a single instance (one writer), for deployments that do not scale out.
- One store kind per service: the Variants are alternatives, so a service with durable state picks exactly one.
- Boundary with VP-C002: the test is whether losing the data is acceptable, not which technology holds it.

### VP-C002 TransientStore
Data the service can survive losing — temporary state, short-lived sessions, derived or recomputable values, and caches in front of slower sources. For some data this is the **primary** home, not a copy of VP-C001 data: a cache is one use of this store, not its definition.
- **None** — no transient state beyond a single request.
- **Redis** — a separate server; shared across service instances; loses data only on failover or eviction.
- **InMemory** — inside the service process; per instance, lost on every restart; binds any state that must be shared across requests to a single instance.
- One store kind per service, for the same reason as VP-C001.
- Independent of VP-C001: a service may have either, both, or neither.

## Bound stack maps

Every map below carries every row of the table above in its own `## Common Variation Points` table. Plain paths, not links — this file belongs to a stack-agnostic skill.

- `skills/go/architecture/variability-map.md`
- `skills/dotnet/architecture/variability-map.md`
