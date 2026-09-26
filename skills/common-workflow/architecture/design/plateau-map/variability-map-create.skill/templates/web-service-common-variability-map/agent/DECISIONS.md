# Decisions log

One line per non-mechanical choice. ⚠️ = a genuine architectural fork, waiting on the owner.

## Owner-decided (2026-09-26, chat)

- TaskBox is the deferred-execution mechanism; Outbox is the policy "outbound calls go through TaskBox". One VP for TaskBox; its store realization depends on where the data lives (Redis data → Redis TaskBox, PostgreSQL data → PostgreSQL TaskBox).
- Redis TaskBox with Redis-resident data is a real outbox (same-store atomic write). No Redis library generally enqueues inside the caller's `MULTI`, so Redis is a shared contract + thin per-stack implementation.
- Common VPs are inherited by every bound stack map under a `VP-C###` ID; the row is always present, even when the stack adds nothing.
- **Rebuild from zero, one VP at a time.** The old template (derived mechanically, never verified on a stack) is discarded; each VP is admitted only after discussion, and designed on every bound stack at admission time so the stack realizations follow one concept.
- **Per-stack depth at admission:** State + concrete realization choice with reasoning + narrowing; a skeleton solution when the stack has none yet.
- **States:** Inherited / Refined / `Fixed: {Variant}` (replaces the earlier `N/A`, which could not express "always Yes").
- **HTTP inbound is mandatory for every backend service** → common baseline, not a VP; gRPC is optional. Applied at backlog item 4 (dotnet's `Module`-level VP8 is the open point there).

## Agent decisions

- New folder `web-service-common-variability-map`, symmetric with `feature-map-create`'s `web-service-common-features`.
- Stack common-VP rows restate nothing the common map owns — duplication is the drift source being removed.
- Stack-local VPs keep their `VPn` IDs; only VPs covered by a common VP are re-IDed, at that VP's admission. Minimises churn (~130 dotnet files reference VP numbers).
- Bound stacks are listed as plain backticked paths, not links — skill-design forbids a stack-agnostic skill linking stack-specialized files.
- Earlier chat answer called TaskBox's variants "At least one (Redis/PostgreSQL)". Corrected: a combinable multi-variant VP violates `variability-map-create`'s "combinables split" rule. TaskBox is Yes/No; *which store* is a Realization-depends-on consequence of the storage VPs — matches the owner's own framing.
- Harness files live in `agent/` beside the common map, matching the go/dotnet catalogs' precedent.
- The earlier forks F1 (DomainLogic), F2 (guarantee VPs), F4 (dotnet domain VPs), F5 (Go ExternalIntegration) are no longer framework questions — each is the open question of its backlog item in INVARIANTS §5.
- `check.sh` checks links only in files this task owns, and in a bound stack map only its `## Common Variation Points` section. **Finding:** `skills/dotnet/architecture/variability-map.md` already has 4 broken links, all into the removed `architecture/v3/` tree (`v3/README.md`, `v3/variability-map.md` ×2, a `v3/plateau/.../adr` file) — pre-existing, left for dotnet's own migration waves, not fixed silently here.
- `check.sh` was mutation-tested against a deliberately broken common map + stack maps (duplicate ID, missing concept section, uncarried VP, unknown VP, bad `Fixed` variant, empty delta, dead link, dead anchor, leftover re-IDed ID) — every case caught, files restored.
- A retired common VP stays in the common map (ID never reused) and is dropped from stack maps.
- Admission step 3's skeleton solution is authored through `solution-create`, so `variability-map-create` keeps its "never writes solution content" principle.
