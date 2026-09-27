# VP-C001 PersistentStore

Concept of a common Variation Point; its question, Variants, Constraint, and Realization depends on are the VP-C001 row of [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map|web-service common map]].

The system of record: **data that survives restarts and redeploys and is never deliberately discarded**. Business changes are written in transactions, which later VPs (TaskBox, Outbox) rely on to stay atomic with the change they report.
- **None** — the service owns no durable state (stateless, or delegates state to other services).
- **PostgreSQL** — a separate database server; fits any number of service instances.
- **SQLite** — an embedded database file; binds the service to a single instance (one writer), for deployments that do not scale out.
- One store kind per service: the Variants are alternatives, so a service with durable state picks exactly one.
- Boundary with VP-C002: the test is whether losing the data is acceptable, not which technology holds it.
