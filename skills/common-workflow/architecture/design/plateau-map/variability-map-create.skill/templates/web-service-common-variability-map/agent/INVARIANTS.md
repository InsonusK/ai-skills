# Web-service common Variability Map — invariants

The anchor document for introducing a **shared, inherited** Variability Map for every backend web-service catalog (per [[skills/common-workflow/bulk-authoring-harness.skill/bulk-authoring-harness.skill.md|bulk-authoring-harness]]). Every artifact this task produces or migrates must satisfy every invariant here. `check.sh` enforces the mechanical ones; the per-wave audit enforces the rest. Open forks are in `DECISIONS.md` (⚠️ entries).

**Problem being fixed.** The existing `templates/web-service-variability-map/` is a *copy-what-applies* menu. Each stack copied, renamed, and re-cut it: Go has 7 VPs, dotnet 14; Go's `ExternalIntegration` is one VP where dotnet splits by transport; `DomainLogic` and `HttpApi` are common baseline in Go and VPs in dotnet. Nothing ties a stack row back to the shared question, so nothing detects drift.

## 1. The model: common VPs are inherited, not copied

- **One common map** at `variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map.md` (renamed from `web-service-variability-map/`, symmetric with `feature-map-create.skill/templates/web-service-common-features/`). It owns, for every common VP: the question, Variants, Constraint, Realization depends on. It never holds `Realized by`.
- **Every bound stack map carries every common VP** — present, by ID, in its own `## Common Variation Points` table. Absent row = check failure, never "not relevant".
- **A stack map restates nothing the common map owns.** Its common-VP row holds only: ID, name, State, the stack's delta (if any), `Realized by`, `Migration`. The question/Variants/Constraint are read from the common map through the ID link. (Every fact stated once — this is what stops the drift.)
- **Stack-local VPs** (variability only this family has) live in a separate `## Stack Variation Points` table with the full column set, exactly as today.

## 2. IDs

| Kind | Format | Example |
| --- | --- | --- |
| Common VP | `VP-C` + 3 digits | `VP-C014` |
| Stack-local VP | `VP` + number (unchanged) | `VP3` |

- IDs are stable forever: never renumbered, never reused. A retired common VP keeps its row with state `Retired` in the common map; its number is not reassigned.
- A stack-local VP that becomes common is **re-IDed** to its `VP-C` number in the stack map and every reference in that stack's tree (see §6). Its old local number is left as a gap, never reused.

## 3. States of a common VP in a stack map

| State | Meaning | Stack delta cell | `Realized by` |
| --- | --- | --- | --- |
| **Inherited** | Taken as defined in the common map. | `—` | Solution link(s) per Variant, or `deferred — {reason}` |
| **Refined** | Taken, plus stack-evidenced narrowing: a Variant unsupported (with reason), an extra Constraint, or a stack-specific realization note. | Required: what is narrowed and why | Solution link(s) per supported Variant |
| **Fixed: {Variant}** | This family's Feature Model makes the answer non-optional — every member answers `{Variant}`. `Fixed: No` is the old "N/A". | Required: the reason (Feature Model ref) | Baseline solution for `Fixed: Yes`; `—` for `Fixed: No` |

- **A stack may narrow, never widen.** It may mark a common Variant unsupported; it may not add a Variant or loosen a Constraint. A new Variant that is not stack-specific is added to the common map first.
- **`deferred`** means "applicable, no solution yet" — a check *warning*, not a failure. It is different from `Fixed: No` ("this family never answers Yes").

## 4. Common VP list (v1 — review this)

Derived from `web-service-common-features` + the owner's TaskBox/Outbox decisions (2026-09-26). F-refs are open forks in `DECISIONS.md`.

| ID | VP | Variants | Constraint | Realization depends on |
| --- | --- | --- | --- | --- |
| VP-C001 | **DomainLogic** — a real domain layer (guarded state transitions), vs. a pass-through service | Yes / No | — | — (F1) |
| VP-C002 | **PersistentStorage** — durable system-of-record store | Yes / No | — | — |
| VP-C003 | **PersistentStoreKind** | PostgreSQL / SQLite | Applicable only when C002=Yes | — |
| VP-C004 | **CacheStorage** — non-durable, cache-capable store | Yes / No | — | — |
| VP-C005 | **CacheStoreKind** | Redis / InMemory | Applicable only when C004=Yes | — |
| VP-C006 | **HttpInbound** | Yes / No | — | — |
| VP-C007 | **GrpcInbound** | Yes / No | — | — |
| VP-C008 | **KafkaConsumer** | Yes / No | — | Mandatory sub-feature: stack messaging infrastructure (shared with C009, C012, C013) |
| VP-C009 | **RabbitMqConsumer** | Yes / No | — | same as C008 |
| VP-C010 | **HttpOutbound** — sync call to another service over HTTP | Yes / No | — | — |
| VP-C011 | **GrpcOutbound** | Yes / No | — | — |
| VP-C012 | **KafkaProducer** | Yes / No | — | same as C008 |
| VP-C013 | **RabbitMqProducer** | Yes / No | — | same as C008 |
| VP-C014 | **TaskBox** — deferred execution: a task is stored, then executed by a background worker | Yes / No | Yes requires (C002=Yes OR C005=Redis) | **One realization per store that holds data a task is created about**, in *that* store: PostgreSQL → stack library with in-transaction enqueue; Redis → common Redis-Streams contract (enqueue inside the caller's `MULTI`); SQLite → stack decides (may be Refined-unsupported); InMemory → none (tasks would not survive restart). The durability guarantee is a consequence of the store, not a separate choice (F2). |
| VP-C015 | **Outbox** — outbound calls are not made directly; they are enqueued as TaskBox tasks and dispatched by the task handler | Yes / No | Yes requires C014=Yes AND (C010 OR C011 OR C012 OR C013 = Yes) | The task is enqueued in the **same atomic write, in the same store,** as the business change it reports; the handler calls a C010–C013 adapter (protocol = FDN, never a new VP); delivery is at-least-once, so every message carries an idempotency key. |
| VP-C016 | **Metric** — stub | Yes / No | — | — |

**Removed vs. the old template:** `HasCriticalGuaranteeMessages` / `HasNonCriticalGuaranteeMessages` (F2). **Moved in the feature template:** `TaskBox` leaves `InboundAsync` and becomes its own optional feature; `Outbox -. Requires .-> TaskBox` stays.

## 5. Bound stacks and ID mapping

Bound stacks are listed in the common map's `## Bound stack maps` section; `check.sh` reads that list. Angular catalogs are a different Program Family (frontend) and are **not** bound.

| Common | Go (`skills/go/architecture`) | dotnet (`skills/dotnet/architecture`) |
| --- | --- | --- |
| C001 DomainLogic | Fixed: Yes (common in Go FM) | ← VP1, Inherited |
| C002 PersistentStorage | ← VP7, Inherited | ← VP2, Refined (+ requires C001=Yes) |
| C003 PersistentStoreKind | new, Refined (PostgreSQL only, ADR postgres-via-pgx) | new, Refined (PostgreSQL only) |
| C004 CacheStorage | ← VP6, Inherited | new, deferred |
| C005 CacheStoreKind | new, Refined (Redis only) | new, deferred |
| C006 HttpInbound | Fixed: Yes (common in Go FM) | ← VP8, Inherited |
| C007 GrpcInbound | ← VP1, Inherited | ← VP9, Inherited |
| C008 KafkaConsumer | ← VP5, Inherited | ← VP12, Inherited |
| C009 RabbitMqConsumer | new, deferred | new, deferred |
| C010 HttpOutbound | new, deferred | ← VP10, Inherited |
| C011 GrpcOutbound | ← VP2 `ExternalIntegration`, Inherited (F5) | ← VP11, Inherited |
| C012 KafkaProducer | ← VP3, Inherited | ← VP13, Inherited |
| C013 RabbitMqProducer | new, deferred | new, deferred |
| C014 TaskBox | new, deferred | new, deferred |
| C015 Outbox | ← VP4, Refined (Kafka dispatch, PostgreSQL only; owner rule on when outbox is mandatory) | ← VP14, Refined (Kafka dispatch only) |
| C016 Metric | new, deferred | new, deferred |
| *stack-local, kept* | none | VP3 ValueObjects, VP4 SharedRules, VP5 EntityConcurrencyControl, VP6 ExternalIdentity, VP7 AuditTimestamps (F4) |

## 6. What a migrated stack must change

1. `variability-map.md`: split into `## Common Variation Points` (all 16 rows, §3 shape) and `## Stack Variation Points` (local rows, unchanged IDs); notes sections re-keyed to the new IDs.
2. Every `VPn` reference to a migrated VP anywhere under that stack's catalog → its `VP-C` ID (plateau skills, `plateau/plateau-repository.md`, solutions, `agent/`, registry, `delta-conflict-analysis.md`). Mechanical, by the §5 mapping; `check.sh` fails on any leftover migrated `VPn`.
3. `feature/feature-model.md` is **not** restructured in this task — only a note that its VPs are now keyed by the common map.

## 7. Skill changes

- `variability-map-create`: the common-map inheritance rules (§1–§3), the explicit exception to "A row only for a real decision" (common rows are always present — a non-decision is `Fixed`), workflow step 2 points at the renamed template; ADR `common-vps-inherited-by-id` recording this vs. copy-menu / per-stack-free alternatives.
- `variability-map.template.md`: two-table shape.
- `feature-map-create`'s `web-service-common-features`: `TaskBox` as its own feature, guarantee nodes removed (per F2), DomainLogic moved per F1; its link to the renamed template.

## Out of scope

- The TaskBox **contract** itself (port `Enqueue(tx, task)`, retry/DLQ/idempotency semantics, the Redis-Streams key/field model, Cucumber scenarios) and any per-stack TaskBox solution — a follow-up task; here C014 only states what the realization depends on.
- Restructuring stack Feature Models to inherit common features the same way (the same problem one layer up — follow-up).
- Angular catalogs.
