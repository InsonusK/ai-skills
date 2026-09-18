---
tags:
  - concern/architecture
  - stack/go
---

# skills/go/architecture Plateau Repository

Maintained per [[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/plateau-map-create.skill.md|plateau-map-create]] as a derived, checkable view over [[skills/go/architecture/variability-map.md|variability-map.md]] — the map is the source of truth for every VP fact (constraints, realizations, variants); this file only re-presents the map plateau-oriented.

Five plateaus exist on disk today, all built by [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]], each `standalone: true` (independently composable as a real service, not merely a staging step toward the next one) and each the direct `parent_plateaus` of the next: `plateau-http-service` → `plateau-dual-api-service` → `plateau-integrated-service` → `plateau-cached-service` → `plateau-persistent-service`.

## Plateau × VP matrix

Rows = plateaus, columns = the 7 Variation Points from `variability-map.md`. ✅ = the VP is realized at that plateau, ❌ = it is not. Answers are **cumulative** down the chain — a plateau has every VP its parent has, plus its own. Scan a **column** for the shallowest plateau that includes a VP; read a **row** for a plateau's complete VP set.

| Plateau | VP1 | VP2 | VP3 | VP4 | VP5 | VP6 | VP7 |
|---|:-:|:-:|:-:|:-:|:-:|:-:|:-:|
| plateau-http-service | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| plateau-dual-api-service | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| plateau-integrated-service | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| plateau-cached-service | ✅ | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ |
| plateau-persistent-service | ✅ | ✅ | ❌ | ❌ | ❌ | ✅ | ✅ |

Column legend — VP1 GrpcApi · VP2 ExternalIntegration · VP3 AsyncOutboundApi (skeleton) · VP4 OutboxPattern (skeleton) · VP5 AsyncInboundApi (skeleton) · VP6 CachedDb · VP7 PersistentDb. Full descriptions, the solution that realizes each VP, and the constraints between VPs are in [[skills/go/architecture/variability-map.md|../variability-map.md]] — the single source of truth; this table is only the plateau-oriented view of the same answers.

- **VP3, VP4, VP5 are ❌ in every plateau, by design, not by omission.** Their solutions (`solution-go-messaging-infrastructure`, `solution-go-kafka-producer`, `solution-go-transactional-outbox`, `solution-go-kafka-consumer`) are skeletons carrying a `> Draft contract` marker — no plateau in this catalog's build composes them yet. See [Combinations the family allows that no plateau covers yet](#combinations-the-family-allows-that-no-plateau-covers-yet).
- **VP1/VP2/VP6/VP7 are plain boolean, catalog-wide** — unlike the worked example's per-entity `Channel` VP, every VP in this catalog is a module-wide Yes/No (the module either exposes gRPC or it doesn't; there is no per-entity variation), so a ✅ here means exactly what it says with no further qualification needed.

## Lineage & new solutions

| # | Plateau | `standalone` | Parent | New solutions in its `created_by` (on top of the parent chain) |
|---|---------|--------------|--------|---------------------------------------------------------------|
| 1 | **plateau-http-service** | `true` | — | `solution-go-repository-structure`, `solution-go-domain-logic`, `solution-go-http-api`, `solution-go-app-logging`, `solution-go-conformance-testing` (none VP-realizing — this is the common baseline every other plateau builds on) |
| 2 | **plateau-dual-api-service** | `true` | plateau-http-service | VP1 `solution-grpc-api` |
| 3 | **plateau-integrated-service** | `true` | plateau-dual-api-service | VP2 `solution-external-integration` |
| 4 | **plateau-cached-service** | `true` | plateau-integrated-service | VP6 `solution-cached-db` |
| 5 | **plateau-persistent-service** | `true` | plateau-cached-service | VP7 `solution-persistent-db` |

`solution-go-domain-ports` (the shared prerequisite that creates an empty `internal/domain/interfaces` package) is not listed above — it is a dependency of the first VP-realizing solution applied at each lineage branch, not a plateau's own `created_by` entry; see its own skill's `adr/shared-prerequisite-not-common-baseline.md`.

## Constraint check

`variability-map.md` states exactly one Constraint: **VP4 `Yes` requires (VP3 `Yes` AND VP7 `Yes`)**, jointly AND.

- No plateau sets VP4 `Yes` (it is ❌ everywhere — VP4 is a skeleton, not yet realized by any plateau) — the constraint is vacuously satisfied in every row.
- `plateau-persistent-service` is the first (and only, so far) plateau with VP7 `Yes`; since it still has VP3 `❌`, the constraint's precondition is not even met there, and correctly, VP4 stays ❌.
- No violations.

## Combinations the family allows that no plateau covers yet

- **Any VP3/VP4/VP5 combination** — Kafka publish, the outbox pattern, and Kafka consume — has no plateau at all. This was a deliberate scope decision for this build: the family's owner specified five plateaus (`base` → `+GrpcApi` → `+ExternalIntegration` → `+CachedDb` → `+PersistentDb`) and explicitly excluded the three Kafka-related options from this batch. A sixth-plateau (or branching) effort to realize `solution-go-messaging-infrastructure`/`solution-go-kafka-producer`/`solution-go-kafka-consumer`/`solution-go-transactional-outbox` past their current skeletons, and compose at least one plateau with VP3 or VP5 `Yes`, is future work, not an oversight.
- **`VP1 ❌` combined with `VP2/VP6/VP7 ✅`** — a service with the external/cache/persistence capabilities but only the common HTTP API, no gRPC — is legal per the map (no Constraint ties VP1 to VP2/VP6/VP7) but has no dedicated plateau, because every plateau past `plateau-http-service` inherits VP1 `✅` from `plateau-dual-api-service` onward. A team wanting persistence without gRPC would need a new lineage branch off `plateau-http-service` directly, or a plateau built by hand-picking `solution-external-integration`/`solution-cached-db`/`solution-persistent-db` without `solution-grpc-api` — `variability-map.md` permits this combination even though this build's linear plateau chain does not currently include it.
- **`VP6 ✅` with `VP7 ❌`, or the reverse in isolation without the other's predecessor** — both are individually legal (VP6 and VP7 carry no Constraint between them), but this catalog's linear chain only ever composes them in the order `..., +CachedDb, +CachedDb+PersistentDb` — there is no plateau with `PersistentDb` alone, without `CachedDb`. The reference implementation `tmp/tg-bot-service` that seeded this catalog's build is noted (per `agent/DECISIONS.md`) as having a DB that is "cached AND persistent" simultaneously, which is exactly what `plateau-persistent-service` demonstrates — but a `PersistentDb`-only plateau (no cache in front of the reputation lookup) is equally legal per the map and simply hasn't been built.
