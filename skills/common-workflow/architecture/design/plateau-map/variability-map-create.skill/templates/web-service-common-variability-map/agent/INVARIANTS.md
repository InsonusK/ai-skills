# Web-service common Variability Map — invariants

The anchor document for replacing the copy-what-applies `templates/web-service-variability-map/` with a **shared, inherited** common map that every backend web-service catalog carries, built up **one VP at a time** (per [[skills/common-workflow/bulk-authoring-harness.skill/bulk-authoring-harness.skill.md|bulk-authoring-harness]]). `check.sh` enforces the mechanical invariants; the per-VP audit enforces the rest. Choices are logged in `DECISIONS.md`.

**Problem being fixed.** The old template was derived mechanically from the common Feature Model and never verified against a real stack. Each stack copied and re-cut it — Go has 7 VPs, dotnet 14; Go's `ExternalIntegration` is one VP where dotnet splits by transport; `DomainLogic`/`HttpApi` are baseline in Go and VPs in dotnet — and nothing ties a stack row back to a shared question, so nothing detects drift.

## 1. The model: common VPs are inherited, not copied

- **One common map** at `variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map.md`. It starts **empty** and grows only through §4. The old `web-service-variability-map/` is deleted, not kept beside it — two sources of common VPs is the problem being fixed.
- **The common map owns the concept.** Per common VP: a table row (question, Variants, Constraint, Realization depends on) and a `### VP-C### {Name}` section with the concept — what the question means, where its boundary with neighbouring VPs lies, why each Constraint holds. It never holds `Realized by`.
- **Every bound stack map carries every common VP** in its own `## Common Variation Points` table. An absent row is a check failure, never "not relevant".
- **A stack row restates nothing the common map owns.** It holds only: ID (linking the common section), name, State, the stack's delta, `Realized by`, `Migration`. Every fact stated once — this is what stops the drift.
- **Stack-local VPs** (variability only that family has) stay in `## Stack Variation Points` with the full column set, as today.
- **Bound stacks are listed as plain backticked paths** in the common map's `## Bound stack maps` section — never links: the common map lives in a stack-agnostic skill, which must not link stack-specialized files (skill-design). `check.sh` reads that list. Angular catalogs are a different Program Family and are not bound.

## 2. IDs

| Kind | Format | Example |
| --- | --- | --- |
| Common VP | `VP-C` + 3 digits, assigned in admission order | `VP-C001` |
| Stack-local VP | `VP` + number (unchanged) | `VP3` |

- IDs are never renumbered or reused. A retired common VP keeps its common row, marked `Retired` in its VP cell, and is dropped from every stack map.
- When a stack-local VP is covered by a newly admitted common VP, it is **re-IDed** to the `VP-C` number in its stack map and in every reference in that stack's tree. The old local number becomes a gap. `agent/id-map.tsv` records every re-ID; `check.sh` fails on any leftover old ID.

## 3. States of a common VP in a stack map

| State | Meaning | Stack delta cell | `Realized by` |
| --- | --- | --- | --- |
| **Inherited** | Taken as defined in the common map. | `—` | Solution link per Variant, or `deferred — {reason}` |
| **Refined** | Taken with stack-evidenced narrowing: a Variant unsupported, an extra Constraint, a stack-specific realization note. | Required: what is narrowed and why | Solution link per supported Variant |
| **Fixed: {Variant}** | This family's Feature Model makes the answer non-optional — every member answers `{Variant}`. `Fixed: No` = "this family never has it". | Required: reason, with the Feature Model reference | Baseline solution for `Fixed: Yes`; `—` for `Fixed: No` |

- **A stack narrows, never widens.** It may mark a Variant unsupported or add a Constraint; it never adds a Variant or loosens a Constraint. A missing Variant that is not stack-specific goes into the common map first.
- **`deferred`** = applicable, no solution yet → a check *warning*. Distinct from `Fixed: No`.

## 4. Admitting one common VP (the unit of work)

One VP — or a tight group that only makes sense together — per cycle, per commit:
1. **Discuss with the owner:** question, Variants, Constraint, Realization depends on, boundary with neighbouring VPs. Nothing is admitted on the agent's inference alone.
2. **Common map:** add the table row + `### VP-C###` concept section. It may reference only already-admitted common VPs — this fixes the admission order.
3. **Every bound stack, while the concept is fresh:** decide the State, the concrete realization (library or own implementation, and why), and what is narrowed. Record it as a `Realized by` link; when the stack has no solution skill for it yet, create a skeleton solution (`> Draft contract` marker, as the Kafka/Outbox skeletons are) carrying that decision.
4. **Migrate:** a stack-local VP now covered by this common VP is re-IDed (§2) in the same commit.
5. `check.sh` clean → fresh-eyes audit against this file + the governing skills → commit.

## 5. Admission backlog (candidates, not contracts)

Order follows references: a VP comes after every common VP it names.

| # | Candidate | Carries forward from the chat / old template | Open question for its admission |
| --- | --- | --- | --- |
| 1 | Storage: persistent + kind, cache + kind | old VP1–VP4; Go VP6/VP7, dotnet VP2 | Yes/No + kind VP, or one categorical VP? SQLite kept? |
| 2 | TaskBox | deferred execution; realized in the store that holds the task's data (PostgreSQL → library with in-transaction enqueue; Redis → shared Redis-Streams contract, enqueue inside the caller's `MULTI`) | Does the Critical/NonCritical guarantee stay a VP or become a consequence of the store? |
| 3 | Outbox | outbound calls go through TaskBox; same atomic write, same store as the business change; at-least-once + idempotency key | Go VP4 / dotnet VP14 today require Kafka — generalize to any outbound protocol? |
| 4 | Inbound protocols | owner: HTTP is mandatory for every backend service → common baseline, not a VP; gRPC optional | dotnet's family is a `Module` — can a module lack HTTP (dotnet VP8)? |
| 5 | Outbound protocols | old VP10–VP13; dotnet VP10/VP11; Go VP2 `ExternalIntegration` (gRPC-only realization) | Go's transport-agnostic ExternalIntegration → the gRPC VP? |
| 6 | Messaging (Kafka/RabbitMQ consume/produce) | Go VP3/VP5, dotnet VP12/VP13 | Shared messaging infrastructure as mandatory sub-feature |
| 7 | DomainLogic | dotnet VP1; Go baseline | Common VP with Go `Fixed: Yes`, or dotnet-local? |
| 8 | Metric | old VP17 (stub) | Admit now or when a stack needs it? |
| 9 | Domain-modelling (ValueObjects, SharedRules, concurrency, external identity, audit timestamps) | dotnet VP3–VP7 | Stay dotnet-local until a second stack needs one? |

## 6. Framework wave (before any VP)

- Common map with an empty `## Common Variation Points` table, the `## Bound stack maps` list (Go, dotnet), and no VP sections.
- `variability-map-create`: rules for §1–§4 (carry every common VP; restate nothing; narrow never widen; admit through the common map; the states vocabulary), the explicit exception to "A row only for a real decision" (a common row is always present — a non-decision is `Fixed`), workflow step 2 pointing at the new map; ADR `common-vps-inherited-by-id`.
- `variability-map.template.md`: two-table shape.
- Bound stack maps: add an empty `## Common Variation Points` section; rename their table's section to `## Stack Variation Points`. No row moves yet.
- Delete the old template; fix its two inbound links.

## Out of scope

- The TaskBox contract (port `Enqueue(tx, task)`, retry/DLQ/idempotency semantics, the Redis-Streams key/field model, Cucumber scenarios) — authored with or after TaskBox's admission, not in the framework wave.
- Restructuring stack Feature Models to inherit common features the same way (the same problem one layer up).
- Angular catalogs.
