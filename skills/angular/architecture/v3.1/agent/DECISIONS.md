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

## Stage 2 — variability-map-create — pending (4 maps)

## Stage 3 — solution migration + delta-conflict-detection — pending

## Stage 4 — plateau-create-by-solutions — pending (depth TBD)
