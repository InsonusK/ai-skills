---
tags:
  - concern/architecture
  - stack/go
---

# skills/go/architecture Plateau Repository

Maintained per [[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/plateau-map-create.skill.md|plateau-map-create]] as a derived, checkable view over [[skills/go/architecture/variability-map.md|variability-map.md]] — the map is the source of truth for every VP fact (constraints, realizations, variants); this file only re-presents the map plateau-oriented.

Five plateaus exist on disk today, coded per [[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/plateau-map-create.skill.md#plateau-codes|plateau-map-create's plateau codes]] (`GW` = Go web-service; the folders still carry their pre-code names until they are renamed), all built by [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]], each `standalone: true` (independently composable as a real service, not merely a staging step toward the next one) and each the direct `parent_plateaus` of the next: `plateau-http-service` → `plateau-dual-api-service` → `plateau-integrated-service` → `plateau-cached-service` → `plateau-persistent-service`.

## Plateau × VP matrix

Rows = plateaus by code, with the Title decoding it and the current folder name. Columns = the 5 common VPs (cell = the Variant the plateau realizes) and the 4 stack VPs from `variability-map.md` (✅ = realized at that plateau, ❌ = not). Answers are **cumulative** down the chain — a plateau has every VP its parent has, plus its own. Scan a **column** for the shallowest plateau that includes a VP; read a **row** for a plateau's complete VP set.

| Code | Title | Folder | VP-C001 | VP-C002 | VP-C003 | VP-C004 | VP-C005 | VP1 | VP3 | VP4 | VP5 |
|---|---|---|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|
| GW001.000 | http service | plateau-http-service | None | None | No | No | No | ❌ | ❌ | ❌ | ❌ |
| GW001.001 | dual-API service | plateau-dual-api-service | None | None | No | No | No | ✅ | ❌ | ❌ | ❌ |
| GW005.001 | integrated service | plateau-integrated-service | None | None | No | No | Yes | ✅ | ❌ | ❌ | ❌ |
| GW006.001 | cached service | plateau-cached-service | None | Redis | No | No | Yes | ✅ | ❌ | ❌ | ❌ |
| GW007.001 | persistent service | plateau-persistent-service | PostgreSQL | Redis | No | No | Yes | ✅ | ❌ | ❌ | ❌ |

Column legend — VP-C001 PersistentStore (common) · VP-C002 TransientStore (common) · VP-C003 TaskBox (common) · VP-C004 HttpOutbound (common) · VP-C005 GrpcOutbound (common, the Feature Model's ExternalIntegration) · VP1 GrpcApi · VP3 AsyncOutboundApi (skeleton) · VP4 OutboxPattern (skeleton) · VP5 AsyncInboundApi (skeleton). Full descriptions, the solution that realizes each VP, and the constraints between VPs are in [[skills/go/architecture/variability-map.md|../variability-map.md]] and, for common VPs, the [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map|web-service common map]] — the single source of truth; this table is only the plateau-oriented view of the same answers. The `{common}` numbers come from the [[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/registry/web-service-common-plateaus|common-plateau registry]].

- **VP3, VP4, VP5 are ❌ in every plateau, by design, not by omission.** Their solutions (`solution-go-messaging-infrastructure`, `solution-go-kafka-producer`, `solution-go-transactional-outbox`, `solution-go-kafka-consumer`) are skeletons carrying a `> Draft contract` marker — no plateau in this catalog's build composes them yet. See [Combinations the family allows that no plateau covers yet](#combinations-the-family-allows-that-no-plateau-covers-yet).
- **VP1 is plain boolean, catalog-wide, and the common VPs module-wide** — unlike the worked example's per-entity `Channel` VP, a ✅ or a Variant here means exactly what it says with no further qualification needed.
- **Planned Variants** — VP-C001 `SQLite` and VP-C002 `InMemory` are `planned` in the map; no plateau realizes them until a plateau "existing base + that Variant" is built.

## Stack VP combinations

| `{specific}` | Stack VPs |
|---|---|
| 000 | — |
| 001 | VP1 |
| 002 | ⛔ retired — was VP1 plus the ExternalIntegration stack VP, which became common VP-C005 |

## Lineage & new solutions

| # | Plateau | `standalone` | Parent | New solutions in its `created_by` (on top of the parent chain) |
|---|---------|--------------|--------|---------------------------------------------------------------|
| 1 | **GW001.000** http service | `true` | — | `solution-go-repository-structure`, `solution-go-domain-logic`, `solution-go-http-api`, `solution-go-app-logging`, `solution-conformance-testing-in-go` (none VP-realizing — this is the common baseline every other plateau builds on) |
| 2 | **GW001.001** dual-API service | `true` | GW001.000 | VP1 `solution-grpc-api` |
| 3 | **GW005.001** integrated service | `true` | GW001.001 | VP-C005 `solution-external-integration` |
| 4 | **GW006.001** cached service | `true` | GW005.001 | VP-C002 `Redis` `solution-cached-db` |
| 5 | **GW007.001** persistent service | `true` | GW006.001 | VP-C001 `PostgreSQL` `solution-persistent-db` |

`solution-go-domain-ports` (the shared prerequisite that creates an empty `internal/domain/interfaces` package) is not listed above — it is a dependency of the first VP-realizing solution applied at each lineage branch, not a plateau's own `created_by` entry; see its own skill's `adr/shared-prerequisite-not-common-baseline.md`.

## Constraint check

`variability-map.md` states exactly one Constraint: **VP4 `Yes` requires (VP3 `Yes` AND VP-C001 ≠ `None`)**, jointly AND.

- No plateau sets VP4 `Yes` (it is ❌ everywhere — VP4 is a skeleton, not yet realized by any plateau) — the constraint is vacuously satisfied in every row.
- `GW007.001` is the first (and only, so far) plateau with VP-C001 `PostgreSQL`; since it still has VP3 `❌`, the constraint's precondition is not even met there, and correctly, VP4 stays ❌.
- No violations.

## Combinations the family allows that no plateau covers yet

- **Any VP3/VP4/VP5 combination** — Kafka publish, the outbox pattern, and Kafka consume — has no plateau at all. This was a deliberate scope decision for this build: the family's owner specified five plateaus (`base` → `+GrpcApi` → `+ExternalIntegration` → `+CachedDb` → `+PersistentDb`) and explicitly excluded the three Kafka-related options from this batch. A sixth-plateau (or branching) effort to realize `solution-go-messaging-infrastructure`/`solution-go-kafka-producer`/`solution-go-kafka-consumer`/`solution-go-transactional-outbox` past their current skeletons, and compose at least one plateau with VP3 or VP5 `Yes`, is future work, not an oversight.
- **`VP1 ❌` combined with VP-C005 `Yes` or a store other than `None`** — a service with the external/cache/persistence capabilities but only the common HTTP API, no gRPC — is legal per the map (no Constraint ties VP1 to VP-C005/VP-C001/VP-C002) but has no dedicated plateau, because every plateau past `GW001.000` inherits VP1 `✅` from `GW001.001` onward. A team wanting persistence without gRPC would need a new lineage branch off `plateau-http-service` directly, or a plateau built by hand-picking `solution-external-integration`/`solution-cached-db`/`solution-persistent-db` without `solution-grpc-api` — `variability-map.md` permits this combination even though this build's linear plateau chain does not currently include it.
- **PersistentStore `PostgreSQL` without TransientStore `Redis`** — legal (VP-C001 and VP-C002 carry no Constraint between them), but this catalog's linear chain only ever composes them in the order `..., +CachedDb, +CachedDb+PersistentDb` — there is no plateau with `PersistentDb` alone, without `CachedDb`. The reference implementation `tmp/tg-bot-service` that seeded this catalog's build is noted (per `agent/DECISIONS.md`) as having a DB that is "cached AND persistent" simultaneously, which is exactly what `plateau-persistent-service` demonstrates — but a `PersistentDb`-only plateau (no cache in front of the reputation lookup) is equally legal per the map and simply hasn't been built.
