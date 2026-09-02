# Angular architecture v3.1 — build decisions log

One line per non-mechanical choice made while building the Angular v3.1 catalog. `⚠️` marks a fork that needs the owner's sign-off; everything else is execution against [[INVARIANTS.md]].

The pipeline: `feature-map-create` → `variability-map-create` → (copy + migrate V1 solutions) → `delta-conflict-detection` → `plateau-create-by-solutions`. Output root: `skills/angular/architecture/v3.1/`. V1 input (read-only reference): `skills/angular/architecture/solutions/` + `skills/angular/architecture/plateau/`.

## Settled before the build (owner, this session)

- **Output layout** = full parallel catalog under `v3.1/`: `v3.1/feature/`, `v3.1/variability-map.md`, `v3.1/delta-conflict-analysis.md`, `v3.1/solutions/` (copied + migrated), `v3.1/plateau/` (built later), `v3.1/agent/`. V1 dirs untouched.
- **Phasing** = stages 1–3 now (feature-map, variability-map, delta-conflict + solution migration); stage-4 (`plateau-create`) depth decided after stage 3, once the real plateau count is known.
- **Plateau set** = derived fresh from the Variability Map; the existing 8 V1 plateaus become a reference mapping (old plateau → v3.1 VP answers), names kept where they still map cleanly.
- **Solution migration** = full format migration (whenToUse, `## MUST NOT` → negative `## MUST` bullets, de-Russify, version bump, re-verify `depends_on`/`creates`/`extends`) + a fresh-eyes audit per family, mirroring dotnet v3.1's wave audits.
- **Family scope** = **three feature models** — `platform-app`, `design-system`, `embeddable-app` — each with its own baseline + feature diagram. One Variability Map with three VP blocks. One delta-conflict analysis.
- **Aspirational** = V1 solutions + owner-reviewed aspirational candidates (flagged, no `Realized by`).
- **V1 doubts** = recorded in each family model's `Open questions on V1`, carried forward with a working hypothesis, reviewed as a batch.

## Stage 1 — feature-map-create — DRAFT for review (this session)

Three models written to `v3.1/feature/`:

- **`feature/feature-model.md`** — umbrella: the three families, how they relate (design-system consumed by the other two; embeddable-app loaded by a FederationHost platform-app), shared conventions, cross-family shared solutions flagged for splitting.
- **`feature/platform-app/feature-model.md`** (+ diagram) — root `PlatformApp`. 8 common features (NxWorkspaceStructure, TieredStateManagement, HierarchicalRouting, SignalForms, FacadeClientDataAccess, ConsoleLogging, BusinessLayerTesting, ComponentTesting[flagged]); 7 variable (PerformanceTunedRouting, OfflineReadResilience, OfflineWriteQueue[per-feature, child of ORR], FederationHost, DesignSystemConsumption[child of FED], BackendLogDelivery, Authentication). 7 open questions on V1. 5 aspirational candidates.
- **`feature/design-system/feature-model.md`** (+ diagram) — root `DesignSystem`. 4 common (DesignSystemWorkspace, HybridDesignTokens, EncapsulatedComponentLayer, ComponentTesting); **0 variable**. 1 aspirational: MultiTenantTheming (explicitly deferred by `solution-design-system-tokens`).
- **`feature/embeddable-app/feature-model.md`** (+ diagram) — root `EmbeddableApp`. 3 common (FederationRemoteContract, DesignSystemFederationConsumption, SessionConsumption); **0 variable** (pending open question 3). 1 aspirational: InternalArchitectureAdoption.

### ⚠️ Open questions for the owner (batch review) — from the three models

**platform-app:**
1. `ComponentTesting` common or variable? (working: common, matches V1)
2. `solution-ui-testing` `depends_on` conflates platform + design-system sides → split in delta-conflict (working: split)
3. `Authentication` requires `FederationHost`? V1 says yes; only `SessionContract` publication actually needs it (working: **no hard requirement**, drop the edge)
4. `FederationHost` requires `OfflineReadResilience`? V1 `depends_on` says yes; solution prose says "if also present" (working: **no**, over-strong V1 edge)
5. `solution-lazy-loading-routing` → rename `PerformanceTunedRouting` (working: yes)
6. `libs/shared/state` baseline is a skeleton; concrete slices belong to their features; move `auth.store.ts` example into `Authentication` (working: yes)
7. `notifications` slice (needed by OfflineWriteQueue) is unowned → `OfflineWriteQueue` creates it or a small solution does (working: OfflineWriteQueue creates it)

**design-system:**
8. `HybridDesignTokens` common vs "single-tenant variant of a Theming VP" (working: common now)

**embeddable-app:**
9. `solution-authentication` split → `solution-authentication` (platform) + `solution-session-consumption` (embeddable) (working: split)
10. `solution-platform-embeddability` / `solution-design-system-application` are two-sided → split host vs remote halves (working: split)
11. `SessionConsumption` / `DesignSystemFederationConsumption` truly mandatory, or "mandatory if user-scoped / uses DS"? (working: near-universal, technically optional — owner call decides if the family gets its first VPs)
12. `plateau-embeddable-app`'s `parent_plateau: plateau-platform-monolith` is wrong for a separate-repo family → v3.1 embeddable plateau(s) built from scratch, relationship expressed as cross-family `Requires` (working: yes)

## Stage 2 — variability-map-create — pending

## Stage 3 — solution migration + delta-conflict-detection — pending

## Stage 4 — plateau-create-by-solutions — pending (depth TBD)
