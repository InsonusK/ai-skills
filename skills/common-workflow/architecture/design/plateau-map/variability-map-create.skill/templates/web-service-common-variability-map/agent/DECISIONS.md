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

- **Storage admitted as two categorical VPs** (2026-09-26): VP-C001 PersistentStore `None / PostgreSQL / SQLite`, VP-C002 TransientStore `None / Redis / InMemory`. "Transient", not "Cache": Redis is the primary home of temporary, non-critical data, not only a cache in front of PostgreSQL. SQLite and InMemory stay — used for single-pod services.
- **Storage realizations** (agreed): Go SQLite → `modernc.org/sqlite` behind the persistent-db port; Go InMemory → stdlib `map` + `sync.RWMutex` with TTL; dotnet SQLite → EF Core `Microsoft.EntityFrameworkCore.Sqlite` provider swap; dotnet Redis → `StackExchange.Redis` behind a narrow port (not `IDistributedCache`); dotnet InMemory → `IMemoryCache` behind a narrow port.
- Go's `postgres-via-pgx` ADR rejected SQLite for multi-instance reasons; the owner's single-pod use makes SQLite a legitimate Go variant → Go VP-C001 SQLite is realized/deferred, not Refined-unsupported.

- **No throwaway builds** (2026-09-26): a VP Variant is verified by building a new plateau = an existing base plateau + that VP, not by a temporary build. Detailing a stack records `planned — {chosen realization}`; the solution is written when that plateau is built.
- **VP statuses**: 💡 candidate / 📐 concept (common map), ⏳ pending / ✅ detailed (per stack map). Detail status is per stack — one stack may be detailed while another is not. Candidates move from the harness backlog into the common map.
- **Plateau codes** `{stack}{kind}{common}.{specific}`: letters D/G/P/T for stacks, W/C/A for kinds (web-service, CLI, Angular); `{common}` numbers the common-VP combination in a shared registry kept in `plateau-map-create.skill`; `{specific}` numbers the stack-VP combination (same number = same stack-VP set; `000` = none); the old name becomes the matrix's Title column. Plateau statuses: ✅ built with example, 🔸 built only in another stack, no row = never built.
- **Rename-on-change**: a code changes with its combination (agent's call, delegated by the owner).
- **Physical rename of existing plateaus → GitHub issue** (owner); `gh` unauthenticated here, so the issue text goes to the owner and the follow-up is tracked in STATUS.

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
- Detailing no longer authors skeleton solutions (superseded by the plateau-based verification decision); `deferred` renamed `planned — {chosen realization}` so the decided realization is recorded in the map.
- Candidates carry no `VP-C` ID — a candidate that is merged or split would otherwise burn IDs.
- Kind letter `A` = Angular app with stack `T` (TypeScript), e.g. `TA001.000`.
- Plateau code file/folder form replaces the dot with a hyphen (`plateau-GW003-000`): how `ai-skill-manager` parses dotted `*.skill` names could not be verified here.
- Registry numbers assigned in the order the stacks were migrated (Go first): 001 (None, None), 002 (None, Redis), 003 (PostgreSQL, Redis).
- Catalog `agent/DECISIONS.md` and `agent/logs/` are historical journals: they keep the VP IDs of their time (a note at the top points to `id-map.tsv`) and are excluded from the leftover-ID check. Live contracts (`agent/INVARIANTS.md`) are re-IDed.
- Go VP4's Constraint became `VP3=Yes AND VP-C001 ≠ None` (semantic, not string replace): Outbox needs a transactional store, and both PostgreSQL and SQLite are.
- **Finding:** Go `agent/check.sh` §7 warns "not yet: solution-go-conformance-testing" — pre-existing, it looks for the old name of `solution-conformance-testing-in-go`. Unrelated to this task; left as is.
- dotnet VP-C001 is `Refined`: its stack Constraint (`≠ None` requires stack VP1 DomainLogic) and the repository-backed query handlers stay in the stack delta. dotnet PostgreSQL = the existing EF Core bundle; SQLite = the same bundle with the Sqlite provider (planned).
- dotnet's `plateau-repository.md` "Reference: v3 plateaus" section cites v3's *own* VP numbering; its "`VP2` = Http" was reworded without an ID so the re-ID could not corrupt it. Every other `VP2` in the dotnet tree referred to v3.1 Persistence (checked).
- **Finding:** dotnet `agent/check.sh` is stale — it still targets the removed `v3.1/` paths ("no plateau/ folder yet", "not yet: solution-…" for solutions that exist). Pre-existing; not fixed here.
