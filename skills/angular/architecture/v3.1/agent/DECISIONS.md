# Angular architecture v3.1 — build decisions log

One line per non-mechanical choice made while building the Angular v3.1 catalog. `⚠️` marks a fork that needs the owner's sign-off; everything else is execution against [[INVARIANTS.md]].

Pipeline: `feature-map-create` → `variability-map-create` → (copy + migrate V1 solutions) → `delta-conflict-detection` → `plateau-create-by-solutions`. Output root: `skills/angular/architecture/v3.1/`. V1 input (read-only): `skills/angular/architecture/solutions/` + `skills/angular/architecture/plateau/`.

## Settled before the build (owner, this session)

- **Output layout** = full parallel catalog under `v3.1/`. V1 dirs untouched.
- **Phasing** = stages 1–3 now; stage-4 depth decided after stage 3.
- **Plateau set** = derived fresh from the Variability Maps; existing 8 V1 plateaus become a reference mapping.
- **Solution migration** = full format migration + fresh-eyes audit, mirroring dotnet v3.1's wave audits.
- **Aspirational** = V1 solutions + owner-reviewed aspirational candidates (flagged, no `Realized by`).
- **V1 doubts** = recorded per model in `Open questions on V1`, carried forward with a working hypothesis, batch-reviewed.

## ⚠️ Structural decisions (owner, this session)

- **Round 2 — four catalogs, not one** (was three, then four): `monolith/`, `platform-host/`, `embeddable-app/`, `design-system/`. Each has one concrete baseline + its own `feature/`, `variability-map.md`, `plateau/`. Reason: the four products differ in deployment topology, repository, and workspace tooling; one shared variability space would force every reader to filter rows that don't apply to their product.
  - `platform-host/` **composes** `monolith/` via `parent_plateaus` (a host **is** a monolith + federation). Its own variability space is only the federation delta.
  - `embeddable-app/` and `platform-host/` are two roles of one distributed product but have incompatible baselines (Nx monolith vs. any-tooling contract-conformant) → separate catalogs, related by cross-catalog `Requires`.
  - `@platform/contracts` is owned/published by `platform-host/` (`PlatformContracts` feature), consumed by `embeddable-app/`.
- **One shared solution pool** `v3.1/solutions/` for all four catalogs (avoids duplicating `solution-app-testing` etc.); two-sided V1 solutions are split during delta-conflict-detection.
- **Baseline correction (monolith, first-principles not "what V1 plateaus include"):**
  - `BackendDataAccess` (Facade/Client/`http-core`) is a **VP**, not common — a no-backend app is legitimate. Gates `OfflineReadResilience`, `BackendLogDelivery`, `Authentication`.
  - `GlobalStore` (`libs/shared/state` classical NgRx) is a **VP** (round 3 — was "on-demand artifact"; owner wants it explicit). `Authentication` / `OfflineReadResilience` / `OfflineWriteQueue` `require GlobalStore` (dotnet "VP5/6/7 require VP2" pattern).
  - `StateTieringPolicy` common = the rule + the two lower tiers (component signal, feature Signal Store) only.
  - `SignalForms` + `ConsoleLogging` stay common — zero-cost conventions.
  - `PersistedState` (NgRx state → `localStorage`/IndexedDB across sessions, **not** tokens) — owner-confirmed as a row-to-be; no V1 solution, so a new one is authored at Stage 3. Aspirational until then.

## Stage 1 — feature-map-create — DRAFT for review (this session)

Files under `v3.1/`:

- **`README.md`** — the four-catalog structure, how they relate, the feature→variability→plateau reading guide.
- **`monolith/feature/feature-model.md`** (+ diagram) — root `App`. Common (7): NxWorkspaceStructure, HierarchicalRouting, StateTieringPolicy, SignalForms, ConsoleLogging, BusinessLayerTesting, ComponentTesting[flagged]. Variable (6): PerformanceTunedRouting, GlobalStore, BackendDataAccess, + 3 children of BackendDataAccess (OfflineReadResilience → child OfflineWriteQueue[per-feature]; BackendLogDelivery; Authentication). Requires-edges: ORR/AUTH → GlobalStore; ORR/BLD/AUTH → BackendDataAccess; BLD → ConsoleLogging. 7 open questions. 6 aspirational candidates (incl. PersistedState, owner-confirmed).
- **`platform-host/feature/feature-model.md`** (+ diagram) — root `PlatformHost`, composes `monolith/`. Common (2): RuntimeRemoteFederation, PlatformContracts. Variable (3): HostDesignSystemConsumption, SessionSharing, FederatedReadResilience. Cross-catalog Requires into monolith: SessionSharing→Authentication, FederatedReadResilience→OfflineReadResilience. 6 open questions. 2 aspirational.
- **`embeddable-app/feature/feature-model.md`** (+ diagram) — root `EmbeddableApp`, minimal contract-conformant baseline. Common (1): FederationRemoteContract. Variable (2, near-universal): RemoteSessionConsumption, RemoteDesignSystemConsumption. Aspirational: RemoteInternalArchitecture (composes `monolith/`). 4 open questions.
- **`design-system/feature/feature-model.md`** (+ diagram) — root `DesignSystem`. Common (4): DesignSystemWorkspace, HybridDesignTokens, EncapsulatedComponentLayer, ComponentTesting. **0 variable.** 1 aspirational: MultiTenantTheming. 2 open questions.

### ⚠️ Open questions for the owner (batch review)

**monolith:** (1) modify `solution-repository-structure` — `data-access` split conditional on `BackendDataAccess`; (2) `ComponentTesting` common vs variable (working: common); (3) `solution-ui-testing` splits monolith-side / design-system-side; (4) RESOLVED — `GlobalStore` is a VP; (5) `notifications` slice owned by `OfflineWriteQueue`; (6) rename `solution-lazy-loading-routing` → `PerformanceTunedRouting`; (7) keep `apps/platform-shell` name.

**platform-host:** (1) split `solution-platform-embeddability` → `solution-federation-host` + `solution-platform-contracts` + `solution-federation-remote`; (2) split `solution-design-system-application` host/remote; (3) new `solution-session-sharing` (depends_on monolith `solution-authentication`); (4) `HostDesignSystemConsumption` variable, not a federation prerequisite; (5) V1 `platform-embeddability depends_on offline-first` over-strong → only `FederatedReadResilience` needs it; (6) zero-remote host is valid.

**embeddable-app:** (1) `plateau-embeddable-app`'s `parent_plateau` is wrong — plateaus built from scratch; (2) `RemoteSessionConsumption` / `RemoteDesignSystemConsumption` optional (near-universal) → catalog's first 2 VPs; (3) three V1 solutions split, this catalog gets the remote halves; (4) no own testing/structure solutions.

**design-system:** (1) `HybridDesignTokens` common now, becomes a `Theming` VP variant if `MultiTenantTheming` built; (2) `solution-ui-testing` split (shared with monolith).

## Stage 2 — variability-map-create — DRAFT for review (this session)

Owner reviewed & approved the four feature models; proceeded to Stage 2. Four `variability-map.md` written:

- **`monolith/variability-map.md`** — 7 VPs + 1 aspirational (VP8 PersistedState). VP1 PerformanceTunedRouting, VP2 GlobalStore, VP3 BackendDataAccess, VP4 OfflineReadResilience (req VP3+VP2), VP5 OfflineWriteQueue (per feature, req VP4), VP6 BackendLogDelivery (req VP3), VP7 Authentication (req VP3+VP2). `Migration=Yes` for VP1/VP4/VP5/VP6/VP7 (the V1 plateau chain documents exactly those transitions). Reference table maps the 5 V1 main-chain plateaus; v3.1 additionally allows thinner plateaus (VP2=No / VP3=No / auth-without-federation).
- **`platform-host/variability-map.md`** — 3 VPs (federation delta only; monolith VPs answered by `parent_plateaus`). VP1 HostDesignSystemConsumption, VP2 SessionSharing (req `monolith:VP7`), VP3 FederatedReadResilience (req `monolith:VP4`). Constraint direction inverted from V1 (`solution-authentication depends_on platform-embeddability` → `solution-session-sharing depends_on monolith solution-authentication`).
- **`embeddable-app/variability-map.md`** — 2 real VPs + 1 aspirational. VP1 RemoteSessionConsumption, VP2 RemoteDesignSystemConsumption (both near-universal but variable — owner ruling), VP3 RemoteInternalArchitecture (aspirational, `parent_plateaus` a monolith plateau). Host references are "meaningful only if", not legality gates.
- **`design-system/variability-map.md`** — **0 VPs**. VP1 MultiTenantTheming aspirational only.

### Constraints to encode in `depends_on` at Stage 3 (v3.1 ADRs)

- `solution-offline-first`, `solution-offline-sync`, `solution-authentication` → `depends_on solution-global-store` (monolith VP2 gating).
- `solution-authentication` → `depends_on solution-api-http-layer` (V1 gap; monolith VP3 gating VP7).
- `solution-session-sharing` (new) → `depends_on` monolith `solution-authentication` + `solution-platform-contracts`.
- `solution-repository-structure` modified (data-access lib conditional on VP3) — `solution-update` ADR.

### Solution roster after the Stage-3 splits (planned)

| V1 solution | v3.1 outcome |
| --- | --- |
| `solution-state-management` | split → `solution-state-tiering` (common: rule + 2 tiers) + `solution-global-store` (monolith VP2) |
| `solution-lazy-loading-routing` | renamed → `solution-performance-tuned-routing` (monolith VP1) |
| `solution-repository-structure` | migrated + modified (data-access conditional) |
| `solution-api-http-layer` / `solution-offline-first` / `solution-offline-sync` / `solution-logging-base` / `solution-logging-global` / `solution-app-routing` / `solution-forms` / `solution-app-testing` | migrated as-is (format only) |
| `solution-authentication` | migrated minus SessionContract publication (monolith VP7) |
| `solution-platform-embeddability` | split → `solution-federation-host` + `solution-platform-contracts` (platform-host) + `solution-federation-remote` (embeddable-app) |
| `solution-design-system-application` | split → `solution-host-design-system-consumption` (platform-host VP1) + `solution-remote-design-system-consumption` (embeddable-app VP2) |
| `solution-ui-testing` | split → monolith-side + design-system-side realizations |
| `solution-design-system-structure` / `-tokens` / `-components` | migrated as-is (design-system common) |
| — (new) | `solution-session-sharing` (platform-host VP2), `solution-session-consumption` (embeddable-app VP1), `solution-persisted-state` (monolith VP8), later `solution-design-system-multi-tenant-theming` |

## Stage 3 — solution migration + delta-conflict-detection — DONE (this session)

**3a — mechanical migration** (commit `3072b192`): 18 V1 solutions copied into `v3.1/solutions/` (flattened). 603 link-path rewrites, 18 version bumps, ~135 de-Russifications (labels + prose; 4 `solution-ui-testing/glossary/*` still Russian — tracked debt). `## MUST NOT`/`## SHOULD NOT`/`# Anti-patterns` → negative bullets under `## MUST`/`## SHOULD` (123 files; forbidden-heading count 0). `triggers:` → one-sentence `whenToUse:` (18). adr links → `.md`; non-slug labels → slugs; "solution #N" → slug. `agent/INVARIANTS.md` written.

**3b — state split + rename** (commit `e75f4ba2`):
- `solution-lazy-loading-routing` → `solution-performance-tuned-routing`.
- `solution-state-management` → `solution-state-tiering` (common — rule + 2 lower tiers, new main skill + Boundaries + ADR renamed) + `solution-global-store` (VP2 — `libs/shared/state` + `store.config.ts` seam, new main skill + Boundaries + ADR `classical-ngrx-for-the-global-tier`).
- `auth.store` worked example moved into `solution-authentication`, `.create` + `.extend` merged.
- Inbound refs repointed: forms/api-http-layer/app-testing → state-tiering; offline-first/offline-sync/authentication → global-store.

**3c — federation/design-system splits + new solutions** (commit `9c604484`):
- `solution-platform-embeddability` → `solution-federation-host` (platform-host common; drops over-strong `depends_on solution-offline-first`) + `solution-platform-contracts` (platform-host common; own repo; new main + ADR + Implementation stub) + `solution-federation-remote` (embeddable-app common).
- `solution-design-system-application` → `solution-host-design-system-consumption` (platform-host VP1) + `solution-remote-design-system-consumption` (embeddable-app VP2).
- `solution-authentication` rewritten monolith-scoped: `depends_on` global-store + app-routing + api-http-layer (the V1 api-http-layer gap closed); SessionContract publication carved out.
- NEW: `solution-session-sharing` (platform-host VP2), `solution-session-consumption` (embeddable-app VP1), `solution-persisted-state` (monolith VP8 skeleton, `> Draft contract`), `solution-design-system-multi-tenant-theming` (design-system VP1 skeleton).
- **26 solutions total.** `agent/check.sh` (9 sections) PASS.

**3d — delta-conflict-detection** (this commit): `delta-conflict-analysis.md`. Pre-analysis: `element/repository` split per-catalog (was 3 `.create` on one element — a design error only because the tag conflated 3 different products), `ds-{component}.component.ts` retagged `element/ds-component-ts`, `demo.project.extend` retagged. **8 intersecting groups, ALL canonical — NO `TMC`/`FMC`/`FDC`, zero resolver solutions.** Findings: (1) split `solution-ui-testing` monolith-side / design-system-side (deferred — the one outstanding structural change); (4) `authentication` + `offline-sync` should carry an explicit `shared-state-project` `.extend` registering their slices (Stage-4 / solution-update).

### Debt-closing pass — DONE (this session, commits `6c70acdc` + this one)

- ✅ **`solution-ui-testing` split** (delta-conflict Finding 1) — `solution-ui-testing` (monolith) + `solution-design-system-ui-testing` (design-system, reuses the 3 ADRs + 4 spec patterns). 27 solutions.
- ✅ **Doubled `#MUST`/`#SHOULD` link lists** deduped in 9 main files.
- ✅ **4 `solution-ui-testing/glossary/*.md` + README** translated to English. No Cyrillic anywhere in `v3.1/` (check.sh §2).
- ✅ **`# Boundaries` sections** added to the 13 straight-migrated solutions — all 27 solutions now have one.
- ✅ **`depends_on solution-global-store`** — already present on `offline-first` / `offline-sync` / `authentication` (added during the state-split repoint; the earlier note was stale).
- ✅ **Variability-map Realized-by links** repointed from V1 paths into `v3.1/solutions/` (with renames); Status paragraphs updated.
- ✅ **Inline `MUST`/`MUST NOT`/`SHOULD`/`MAY` in Implementation rule bullets** softened to lowercase / `must never` / `should never` (225 bullets, 72 files) — RFC-2119-shout removed; check.sh §3b heading count stays 0.

### Tracked debt (still deferred — genuinely optional / Stage-4-adjacent)

- `Risk:`/`Fix:` sub-bullets on every Implementation `## MUST` rule bullet (dotnet also deferred this through its wave audits; the negative bullets are correctly placed, just terse).
- `solution-repository-structure` `solution-update` ADR recording the "data-access lib is conditional on VP3" change (currently only in prose + the variability-map).
- The `shared-state-project` `.extend` on `authentication` / `offline-sync` to register their slices in `store.config.ts` (delta-conflict Finding 4) — natural to do during Stage 4 plateau assembly.
- 2 aspirational skeletons (`solution-persisted-state`, `solution-design-system-multi-tenant-theming`) have no Implementation — by design (`> Draft contract — no consumer yet`).
- A `plateau/README.md` + `agent/` polish parallel to dotnet's — deferred to Stage 4.

## Stage 4 — plateau-create-by-solutions — IN PROGRESS

### ⚠️ Owner decisions (this session)

- **Depth**: FULL for `monolith` + `design-system` (runnable examples, Vitest/Playwright green); **limited** for `platform-host` + `embeddable-app` (a federation smoke test, not full apps).
- **Effort**: xhigh, no subagents/workflows (the 4 catalogs are too interdependent — cross-catalog `parent_plateaus`, shared solution pool, cross-catalog registry — for isolated subagents; the build-verify loop is sequential).
- **Plateau set — 8 plateaus:**

| Catalog | Plateau | Composes | Adds | Example |
| --- | --- | --- | --- | --- |
| monolith | `plateau-online-monolith` | — (from scratch) | common + VP2 GlobalStore + VP3 BackendDataAccess | FULL |
| monolith | `plateau-async-monolith` *(renamed from V1 `async-monolith` — "async" read as async-data, confusing)* | online-monolith | VP1 PerformanceTunedRouting (selective preload + `loadComponent` split + bundle budgets) | FULL |
| monolith | `plateau-offline-read-monolith` | perf-routing-monolith | VP4 OfflineReadResilience (Workbox SW, `isOnline`, `OfflineTransportError`) — reads survive offline, writes fail | FULL |
| monolith | `plateau-offline-full-monolith` | offline-read-monolith | VP5 OfflineWriteQueue (Dexie queue, replay, conflict) — reads + writes survive offline | FULL — **owner's current app** |
| monolith | `plateau-multiuser-monolith` | offline-full-monolith | VP6 BackendLogDelivery + VP7 Authentication | FULL — **platform-host's parent** |
| design-system | `plateau-design-system` | — (from scratch) | 4 common | FULL |
| platform-host | `plateau-platform-host` | `plateau-multiuser-monolith` (cross-catalog `parent_plateaus`) | RuntimeRemoteFederation + PlatformContracts + VP1/2/3 | limited (federation smoke test) |
| embeddable-app | `plateau-embeddable-app` | — (from scratch) | FederationRemoteContract + VP1 RemoteSessionConsumption + VP2 RemoteDesignSystemConsumption | limited (trivial remote) |

- `offline-read` / `offline-full` naming: chosen so the family is visually grouped by `offline-` prefix with the partial-vs-complete distinction explicit (V1's `async` / `offline` didn't convey it). `plateau-async-monolith` name kept from V1 (owner-confirmed) — the name is not self-evident, so the plateau's own description makes clear it is about JS chunk-loading strategy, not async data.
- The example **evolves down the chain** (`plateau-create-by-solutions` step 5): child seeds from parent's `example/`, then extends. One Nx workspace, grown plateau by plateau.
- `plateau-create-by-solutions` was **extended to document the Angular / TypeScript stack** (commit `0e2be8cd`) — file-pattern table, project/class name normalization, Angular branches in the build steps.

### Environment (this session)

The container has **no Node**. Installed manually from nodejs.org (npm registry reachable): Node v24.20.0, npm 11.19.0. Angular **22.1.4** is `latest` (Signal Forms available), Nx **23.2.0**, NgRx **22.0.0**. Playwright chromium installed. **The `@playwright/test` runner cannot fork workers in this Bash sandbox** (`chromium.launch()` API works; the test-runner hangs) — so Playwright suites are written + configured but not executed here; Vitest (jsdom) is the runnable gate. Repro steps: `agent/example-setup.md`.

### Status

- **`plateau-online-monolith` — DONE** (commits `37a774b5`, `a52d01b8`, `2fb2e52a`):
  - Root skill (v3.1 format, `standalone: true`, `parent_plateaus: []`, 9-solution `created_by`, VP2=Yes VP3=Yes).
  - `structure/` — 27 skills (1 repo + 11 project + 15 class), seeded from V1, repointed to v3.1/solutions, `created_by` fixed per element, auth removed (VP7), `## MUST NOT`/`# Anti-patterns` converted, 9 missing skills generated from their solution Implementation files.
  - `example/` — runnable Nx 23 / Angular 22.1.4 / NgRx 22 workspace. `orders` feature end to end (Signal Forms form, `OrdersStore` → Facade direct, Facade/Client/Mapper/typed-errors, http-core, LoggerService+ConsoleLogSink). **`npm test` (Vitest, 9 projects) + `npm run lint` GREEN.** Playwright e2e + visual/style-snapshot/a11y specs written & configured (container-safe args, API mocked), documented to run elsewhere.
  - **Catalog feedback**: confirmed `solution-api-http-layer`'s "retry GET only" rule — an early `base-http.service.ts` that retried POST doubled a 409'd write.
  - **Post-commit fix** (`ff98210a`): the committed example did **not** actually pass `nx run-many -t lint` (~19 errors) — the Nx generator's scaffold `@nx/enforce-module-boundaries` `depConstraints` (`scope:shop`/`scope:api`/`type:data`) matched none of this taxonomy's tags, and per-lib component-selector prefixes were left at the generator default `lib`. `eslint.config.mjs` now encodes the real type/scope allow-list (+ two gaps: `data-access → data-access`, `preview → data-access`); `no-console` enabled as an error. The repo skill's allow-list table was updated to match.
- **`plateau-async-monolith` — DONE** (this session):
  - Root skill: `parent_plateaus: [plateau-online-monolith]`, `standalone: true`, `created_by: [solution-performance-tuned-routing]` (VP1). Fixes VP1=Yes, VP2=Yes, VP3=Yes; VP4–VP8=No.
  - `structure/` — seeded from `plateau-online-monolith/structure/` (27 skills, prefix-renamed + `plateau:` swapped + version `20260902160000`), then merged `solution-performance-tuned-routing`: repo skill (bundle-budget + preload rules), `project-platform-shell` (`/preloading/`, `withPreloading`, `project.json` budgets), `class-feature-routes` (`loadComponent` sub-splitting), + **1 new** `class-selective-preloading-strategy`. No new project.
  - `example/` — the online-monolith Nx workspace evolved: `SelectivePreloadingStrategy` (+ spec), `withPreloading(...)`, `data: { preload: true }` on the `orders` mount, a `loadComponent`-split `report` sub-route (own component + `orders.routes.spec.ts` guard), `error`-level `initial`+`anyScript` budgets. **`npm test` (Vitest, 12 files / 23 tests) + `npm run lint` (10 projects) + `nx build platform-shell --configuration=production` all GREEN** — the build output confirms `order-report-component` is its own lazy chunk.
  - **Catalog feedback**: Angular's `anyScript` budget also constrains `main` (initial), not just lazy chunks — the first-draft 300 kB `anyScript` error failed on `main` (301 kB). Budgets tuned so both pass while still catching a real regression.
- **`plateau-offline-read-monolith` — DONE** (this session):
  - Root skill: `parent_plateaus: [plateau-async-monolith]`, `standalone: true`, `created_by: [solution-offline-first]` (VP4). VP1–VP4=Yes; VP5–VP8=No.
  - `structure/` — seeded from `plateau-async-monolith/structure/` (28 skills, renamed, version `20260903090000`), then merged `solution-offline-first`: repo skill (SW / connectivity / OfflineTransportError rules), `project-platform-shell` (SW files + `build-sw` target + banner mount), `project-shared-state` (`connectivity/` slice, no longer "empty"), `project-shared-http-core` (`OfflineTransportError`), `project-shared-ui` (`offline-banner/`), `class-feature-client` (`status === 0` branch), + **3 new** class skills: `class-connectivity-store`, `class-offline-banner-component`, `class-service-worker`. No new project.
  - `example/` — Nx workspace evolved: full `connectivity` classical-NgRx slice + spec; `OfflineTransportError` in http-core + `orders.client.ts` branch + spec; `OfflineBannerComponent` (presentational, `isOnline` input) mounted in the shell; Workbox `sw-src.ts` + pure `sw-routes.ts` (unit-tested) + `sw-build.mjs` + `tsconfig.sw.json` + `build-sw` nx target; `main.ts` registers `/sw.js` post-bootstrap prod-only. **`npm test` (Vitest, 15 files / 44 tests) + `npm run lint` (10 projects) + `npx nx build-sw platform-shell` (emits `dist/.../sw.js`, 7 files precached) all GREEN.** Installed `workbox-{build,precaching,routing,strategies,expiration}` + `esbuild` (already present).
  - **Catalog feedback** (recorded in example README + class skills):
    1. `solution-offline-first`'s `OfflineBannerComponent` sketch injected `Store` inside the component → `type:ui` → `type:store` boundary violation. Made it presentational (`isOnline` input), shell wires the slice.
    2. Workbox SW needs a dedicated `tsconfig.sw.json` (`lib: webworker`) excluded from the app build; routing decisions extracted to a pure `sw-routes.ts` for unit-testability.
    3. `sw-build.mjs` not `.ts` — plain Node ESM (no `.ts` script runner wired), esbuild-bundles `sw-src.ts` first.
- **Plateaus 4–8: not started.** The example evolves down the monolith chain from the offline-read base.

**Multi-session effort** (dotnet's Stage 4 was 3 plateaus over several sessions; this is 8 + evolving examples).
