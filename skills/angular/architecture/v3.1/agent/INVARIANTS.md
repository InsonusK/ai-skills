# Angular v3.1 catalog invariants

Anchor document for the v3.1 solution-catalog build (per [[skills/common-workflow/bulk-authoring-harness.skill/bulk-authoring-harness.skill.md|bulk-authoring-harness]]). Every solution in `v3.1/solutions/` must satisfy every invariant here. `check.sh` enforces the mechanical ones; the fresh-eyes audit enforces the rest.

The shared solution pool `v3.1/solutions/` serves four catalogs — [[skills/angular/architecture/v3.1/README.md|monolith / platform-host / embeddable-app / design-system]]. A solution belongs to whichever catalog(s)' variability-map cites it in **Realized by**.

## 1. Baselines (what exists before any optional feature)

**monolith** — the simplest legitimate Angular app in this architecture:
```
nx.json  package.json  eslint.config.js
apps/platform-shell/            ← solution-repository-structure
apps/platform-shell-e2e/        ← solution-app-testing
libs/shared/ui                  ← solution-repository-structure
libs/shared/util                ← solution-repository-structure
libs/shared/logging             ← solution-logging-base
libs/{feature}/feature          ← solution-repository-structure (feature lib; NO data-access lib yet)
```
- **No `libs/shared/http-core` / no `libs/{feature}/data-access`** — added by `solution-api-http-layer` (VP3 BackendDataAccess).
- **No `libs/shared/state`** — added by `solution-global-store` (VP2 GlobalStore).
- **No service worker / `libs/shared/offline-sync` / `libs/shared/auth-ui` / `BackendLogSink` / `apps/component-preview`** — each from its own VP.

**platform-host** — a monolith baseline (any monolith plateau, via `parent_plateaus`) **plus**: `apps/platform-shell` federation host config, `RemoteRegistryService`, `@platform/contracts` (its own repo). From `solution-federation-host` + `solution-platform-contracts`.

**embeddable-app** — minimal, any tooling: `federation.config`, an exposed module, `singleton` Angular + `@platform/contracts`, hierarchical routing one level down. From `solution-federation-remote`.

**design-system** — Angular CLI multi-project: `projects/design-system` (ng-packagr) + `projects/demo`, Changesets. From `solution-design-system-structure`.

## 2. Common features → realizing solution (must be 1:1 covered)

| Catalog | Common feature | Solution |
| --- | --- | --- |
| monolith | NxWorkspaceStructure | `solution-repository-structure` |
| monolith | HierarchicalRouting | `solution-app-routing` |
| monolith | StateTieringPolicy | `solution-state-tiering` *(carved from `solution-state-management`)* |
| monolith | SignalForms | `solution-forms` |
| monolith | ConsoleLogging | `solution-logging-base` |
| monolith | BusinessLayerTesting | `solution-app-testing` |
| monolith | ComponentTesting | `solution-ui-testing` *(monolith-side; flagged common — feature-model open question)* |
| platform-host | RuntimeRemoteFederation | `solution-federation-host` |
| platform-host | PlatformContracts | `solution-platform-contracts` |
| embeddable-app | FederationRemoteContract | `solution-federation-remote` |
| design-system | DesignSystemWorkspace | `solution-design-system-structure` |
| design-system | HybridDesignTokens | `solution-design-system-tokens` |
| design-system | EncapsulatedComponentLayer | `solution-design-system-components` |
| design-system | ComponentTesting | `solution-ui-testing` *(design-system-side)* |

## 3. Variation Points → realizing solution(s) (must be 1:1 covered)

| Catalog | VP | Solution(s) | Constraint |
| --- | --- | --- | --- |
| monolith VP1 | PerformanceTunedRouting | `solution-performance-tuned-routing` *(renamed from `solution-lazy-loading-routing`)* | — |
| monolith VP2 | GlobalStore | `solution-global-store` *(carved from `solution-state-management`)* | gates VP4/VP5/VP7 |
| monolith VP3 | BackendDataAccess | `solution-api-http-layer` + modifies `solution-repository-structure` | gates VP4/VP6/VP7 |
| monolith VP4 | OfflineReadResilience | `solution-offline-first` | requires VP2 + VP3 |
| monolith VP5 | OfflineWriteQueue | `solution-offline-sync` | requires VP4 |
| monolith VP6 | BackendLogDelivery | `solution-logging-global` | requires VP3 |
| monolith VP7 | Authentication | `solution-authentication` *(minus SessionContract publication)* | requires VP2 + VP3 |
| monolith VP8 | PersistedState *(aspirational)* | `solution-persisted-state` *(new — skeleton)* | requires VP2 |
| platform-host VP1 | HostDesignSystemConsumption | `solution-host-design-system-consumption` *(split from `solution-design-system-application`)* | — |
| platform-host VP2 | SessionSharing | `solution-session-sharing` *(new)* | requires monolith VP7 |
| platform-host VP3 | FederatedReadResilience | `solution-federation-host` *(SW-extension part)* | requires monolith VP4 |
| embeddable-app VP1 | RemoteSessionConsumption | `solution-session-consumption` *(new)* | — |
| embeddable-app VP2 | RemoteDesignSystemConsumption | `solution-remote-design-system-consumption` *(split from `solution-design-system-application`)* | — |
| embeddable-app VP3 | RemoteInternalArchitecture *(aspirational)* | — *(composes a monolith plateau)* | — |
| design-system VP1 | MultiTenantTheming *(aspirational)* | `solution-design-system-multi-tenant-theming` *(new — skeleton)* | requires HybridDesignTokens |

## 4. Link & path conventions

- Every internal link points inside `skills/angular/architecture/v3.1/` or `skills/common-workflow/` — **never** `skills/angular/architecture/solutions/` or `.../plateau/` (the V1 catalog).
- Wikilink form: `[[skills/angular/architecture/v3.1/solutions/solution-forms.skill/solution-forms.skill.md|solution-forms]]`. Frontmatter `depends_on` entries end with `.skill.md` before the `|`.
- Implementation-file links: `[[.../solution-forms.skill/Implementation/{File}.{kind}.md#SECTION|label]]`.
- A solution's folder name, its main file name, and its `name:` field are identical: `solution-{name}.skill` / `solution-{name}.skill.md` / `name: solution-{name}`.
- `depends_on` labels must be English (V1 used Russian labels — de-Russify on migration).

## 5. Frontmatter policy

- `version:` — `20260902000000` for every v3.1 solution created in this build.
- **No `built_on_plateau` field** — v3.1 plateaus are created after the catalog; state the assumed baseline in `# Boundaries` prose. (V1 angular solutions had no such field; keep it that way.)
- `depends_on:` — only real structural dependencies, each resolving to a `solution-*.skill.md` inside `v3.1/solutions/`.
- `whenToUse:` — one concrete sentence (replaces V1's `triggers:` list).
- `tags:` — `skill/architecture/solution`, `solution/{name}`, `stack/typescript`, `framework/angular` (or `framework/angular-material` / `framework/native-federation` where apt), ≥1 `concern/*`. Implementation files: `solution/{name}` + `element/{name}`. ADRs: `solution/{name}` + `concern/documentation` + `concern/documentation/adr` + `stack/typescript`.

## 6. skill-design compliance

- No `## MUST NOT` / `## SHOULD NOT` headings and no `# Anti-patterns` section anywhere (main files — HARD; Implementation files — tracked debt). Convert to negative bullets under `## MUST`/`## SHOULD` with nested `Risk:` / `Fix:`.
- Every `## MUST` bullet that states a rule carries `Risk:` + `Fix:` (audit, not mechanical).
- Name any `# Goal`/`# Core Principle`/`# Rule` bullet over ~20 words as `**{Name}** - ...`.
- Exactly one `# Goal`, one `# Core Principle(s)`, one `# Rule(s)`, one `# Check list` per skill file.

## 7. Per-classification change checklist

**copy (format only):** flatten into `v3.1/solutions/`; rewrite every V1-catalog link → `v3.1/`; `triggers:` → `whenToUse:`; bump `version`; de-Russify `depends_on` labels + Requirements; resolve `depends_on` to v3.1 solutions; `## MUST NOT`/`# Anti-patterns` cleanup.

**copy + modify / rename / split:** all of the above, plus the change recorded in `DECISIONS.md` and a v3.1 ADR when the change is a real decision (`solution-update` for a modify; new ADR folder for a split).

**new:** author via `solution-create`; ADR + glossary as needed. Aspirational new (`solution-persisted-state`, `solution-design-system-multi-tenant-theming`) get a skeleton + `> Draft contract — no consumer yet` note.

## 8. Ground truth

Each catalog's reference example built by `plateau-create-by-solutions` must build and pass its test gate (Vitest + Playwright for monolith/design-system; the federation smoke test for platform-host/embeddable-app). Until then the catalog is "plausible", not "verified". (Stage 4 — depth TBD.)
