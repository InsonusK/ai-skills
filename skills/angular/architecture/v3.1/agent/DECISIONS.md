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

### ⚠️ Structural decisions (owner, this session — round 2)

- **Three catalogs, not one**: `monolith/`, `platform/`, `design-system/` — each with its own `feature/`, `variability-map.md`, `plateau/`. Reason: monolith→platform is a topology change (adds a 3rd repo `@platform/contracts`, 1 deployable → N runtime-discovered remotes) heavy enough that one shared variability space would force every reader to filter half the rows.
- **`platform/` composes `monolith/`**: its plateaus set `parent_plateaus` to the corresponding monolith plateaus (a platform host **is** a monolith + federation). `platform/`'s own variability space is only the federation delta. The V1 standalone "embeddable-app" is absorbed as the **remote role** inside `platform/`.
- **One shared solution pool** `v3.1/solutions/` for all three catalogs (avoids duplicating `solution-app-testing` etc.); two-sided V1 solutions are split during delta-conflict-detection.
- **Baseline correction (monolith)**: `feature-map-create`'s "first principles, not what V1 plateaus include" applied strictly. `libs/shared/state` (global NgRx store) is **not baseline** — it is a shared on-demand artifact created by the first cross-cutting-state feature (like dotnet's `App.Infrastructure`). `BackendDataAccess` (Facade/Client/`http-core`) is a **VP**, not common — a no-backend app is legitimate; it gates `OfflineReadResilience`, `BackendLogDelivery`, `Authentication`.
- **`SignalForms` + `ConsoleLogging` stay common** — zero-cost conventions ("when you build a form / when you log, do it this way"), like dotnet's `SoftValueObjects` / `AppLogging`.

## Stage 1 — feature-map-create — DRAFT for review (this session)

Files under `v3.1/`:

- **`README.md`** — the three-catalog structure, how they relate, shared solution pool.
- **`monolith/feature/feature-model.md`** (+ diagram) — root `App`. Common: NxWorkspaceStructure, HierarchicalRouting, StateTieringPolicy, SignalForms, ConsoleLogging, BusinessLayerTesting, ComponentTesting[flagged]. Variable: PerformanceTunedRouting, BackendDataAccess, and three children of BackendDataAccess (OfflineReadResilience → child OfflineWriteQueue[per-feature]; BackendLogDelivery; Authentication). 7 open questions. 5 aspirational candidates.
- **`platform/feature/feature-model.md`** (+ diagram) — root `Platform`. Two roles (host = a composed monolith + delta; remote = contract-conformant). Common: RuntimeRemoteFederation, PlatformContracts, FederationRemoteContract. Variable host: HostDesignSystemConsumption, SessionSharing, FederatedReadResilience. Variable remote (per remote): RemoteSessionConsumption, RemoteDesignSystemConsumption, RemoteInternalArchitecture[aspirational]. Cross-catalog Requires into monolith: SessionSharing→Authentication, FederatedReadResilience→OfflineReadResilience. 5 open questions.
- **`design-system/feature/feature-model.md`** (+ diagram) — root `DesignSystem`. Common: DesignSystemWorkspace, HybridDesignTokens, EncapsulatedComponentLayer, ComponentTesting. **0 variable.** 1 aspirational: MultiTenantTheming (explicitly deferred by `solution-design-system-tokens`). 2 open questions.

### ⚠️ Open questions for the owner (batch review)

**monolith:** (1) modify `solution-repository-structure` — `data-access` split is conditional on `BackendDataAccess`; (2) `ComponentTesting` common vs variable (working: common); (3) `solution-ui-testing` splits monolith-side / design-system-side; (4) `libs/shared/state` is a shared on-demand artifact, not owned by one solution; (5) `notifications` slice owned by `OfflineWriteQueue`; (6) rename `solution-lazy-loading-routing` → `PerformanceTunedRouting`; (7) keep `apps/platform-shell` name.

**platform:** (1) split `solution-platform-embeddability` → `solution-federation-host` + `solution-platform-contracts` + `solution-federation-remote`; split `solution-design-system-application` host/remote; split `solution-authentication` session parts → `solution-session-sharing` + `solution-session-consumption`; (2) `plateau-embeddable-app`'s `parent_plateau` is wrong — remote plateaus built from scratch; (3) `HostDesignSystemConsumption` variable, not a federation prerequisite; (4) V1 `platform-embeddability depends_on offline-first` is over-strong → only `FederatedReadResilience` needs it; (5) zero-remote platform is valid.

**design-system:** (1) `HybridDesignTokens` common now, becomes a `Theming` VP variant if `MultiTenantTheming` built; (2) `solution-ui-testing` split (shared with monolith).

## Stage 2 — variability-map-create — pending (3 maps)

## Stage 3 — solution migration + delta-conflict-detection — pending

## Stage 4 — plateau-create-by-solutions — pending (depth TBD)
