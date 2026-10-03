# VP-C002 TransientStore

Concept of a common Variation Point; its question, Variants, Constraint, and Realization depends on are the VP-C002 row of [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map|web-service common map]].

A store in which **every entity has a lifetime** (TTL): temporary state, sessions, derived or recomputable values, caches in front of slower sources. Data that has a lifetime lives only here — never in VP-C001 — and for much of it this is the **primary** home, not a copy of VP-C001 data: a cache is one use of this store, not its definition. Losing an entry early (eviction, failover, restart) is the same event as its lifetime expiring, so the service must already survive it.
- **None** — no state with a lifetime beyond a single request.
- **Redis** — a separate server; shared across service instances; loses entries early only on failover or eviction.
- **InMemory** — inside the service process; per instance; every restart ends every lifetime early; binds any state that must be shared across requests to a single instance.
- One store kind per service, for the same reason as VP-C001.
- Independent of VP-C001: a service may have either, both, or neither.
