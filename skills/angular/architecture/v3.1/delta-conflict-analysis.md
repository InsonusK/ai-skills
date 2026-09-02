# Angular v3.1 delta-conflict analysis

Produced per [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]], run across the **whole shared solution pool** (`v3.1/solutions/`, 26 solutions = the maximal catalog). Intersections found by grouping every `Implementation/` file on its `element/{element-name}` tag. Per-plateau `registry/` entries are created during [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]] (Stage 4), placed at the shallowest plateau where all intersecting solutions coexist.

## Classifier (fixed — from the skill)

`Constraint × Category × Kind`: `F/T/-` · `N/D/M/-` · `N/C/-`. Only **`TMC`, `FMC`, `FDC`** need a resolver; every other code is canonical. **The granularity is the method** — two solutions touching different methods/members of a class, or extending in a single direction (create → refine → redirect), is `FMN`/`TMN`, canonical.

## Pre-analysis fixes applied to the catalog

| Fix | Why |
| --- | --- |
| `element/repository` → per-catalog: `element/monolith-repository`, `element/design-system-repository`, `element/embeddable-repository` | **three `.create` on one element** (`solution-repository-structure` creates the Nx monolith workspace, `solution-design-system-structure` the Angular CLI design-system workspace, `solution-federation-remote` the embeddable-app repo) — a design error only because the tag conflated three genuinely different products. Retagging by catalog gives each exactly one `.create`. `solution-federation-host`'s `Repository.extend` retagged `monolith-repository` (a host extends the monolith repo). |
| `element/component-name-component-ts` on `solution-design-system-components`'s `ds-{component}.component.ts` → `element/ds-component-ts` | it is a `projects/design-system/src/lib/{component}/` file in the **design-system** catalog, not the generic feature-lib `{component}.component.ts` that `solution-forms` / `solution-state-tiering` extend in the **monolith** catalog. Different file, different catalog, different kind. |
| `element/demo-project` on `solution-ui-testing`'s `demo.project.extend` → `element/design-system-repository` | it extends the design-system repo's `projects/demo`, part of that repo's structure. |

## Intersecting groups (8) and their classification

| Element | N | Solutions | Code | Status |
| --- | --- | --- | --- | --- |
| `monolith-repository` | 13 | `solution-repository-structure` `.create`; +12 `.extend` (`api-http-layer`, `app-routing`, `app-testing`, `authentication`, `federation-host`, `forms`, `global-store`, `logging-base`, `logging-global`, `offline-first`, `offline-sync`, `performance-tuned-routing`) | `FMN`/`TMN` | **Canonical, benign** — a repo-level bucket: each solution adds an Nx `type:*`/`scope:*` tag value, a boundary-allow-list row, a `libs/` project, or a convention. Adding one more is never a conflict. **N≥3 note (benign — a repo bucket, touched by nearly every feature by design).** |
| `platform-shell-project` | 5 | `.extend` by `app-routing`, `federation-host`, `logging-global`, `offline-first`, `performance-tuned-routing` (project created by `solution-repository-structure` as part of the workspace) | `FMN`/`TMN` | **Canonical, benign** — the composition root. Each `.extend` adds one distinct wiring at bootstrap: `app.routes.ts` mounts, the Native Federation host config, `GlobalErrorHandler` registration, the service-worker registration, the `SelectivePreloadingStrategy`. No two edit the same statement. **N≥3 note (benign).** |
| `design-system-repository` | 4 | `solution-design-system-structure` `.create`; `.extend` by `design-system-components`, `design-system-tokens`, `ui-testing` | `FMN`/`TMN` | **Canonical** — `structure` creates the CLI workspace; `tokens` adds `theme.scss`/`custom-tokens.scss`; `components` adds the `ds-*` selector convention; `ui-testing` adds `projects/demo` visual/a11y targets. Member-disjoint. |
| `feature-client-ts` | 2 | `api-http-layer` `.create` (`{feature}.client.ts`); `offline-first` `.extend` (throw `OfflineTransportError`) | `TMN` | Canonical — `offline-first` (VP4) `requires` `api-http-layer` (VP3); the extend adds one distinct catch branch to a method the create defined. |
| `feature-facade-ts` | 2 | `api-http-layer` `.create` (`{feature}.facade.ts`); `offline-sync` `.extend` (catch `OfflineTransportError` → enqueue) | `TMN` | Canonical — `offline-sync` (VP5) `requires` `offline-first` (VP4) `requires` `api-http-layer` (VP3); the extend adds a distinct per-operation opt-in path. |
| `feature-routes-ts` | 2 | `app-routing` `.create` (`{feature}.routes.ts`); `performance-tuned-routing` `.extend` (`loadComponent` sub-splitting) | `FMN`/`TMN` | Canonical — single-direction refine: `app-routing` (common) defines the feature's root routes; `performance-tuned-routing` (VP1) switches heavy sub-routes to `loadComponent`. |
| `platform-contracts` | 2 | `platform-contracts` `.create` (the package); `session-sharing` `.extend` (adds `SessionContract`) | `TMN` | Canonical — `session-sharing` (platform-host VP2) `depends_on` `platform-contracts`; the extend adds one distinct contract shape. |
| `shared-state-project` | 2 | `global-store` `.create` (`libs/shared/state` + `store.config.ts` seam); `offline-first` `.extend` (register the `connectivity` slice) | `TMN` | Canonical — `offline-first` (VP4) `requires` `global-store` (VP2); the extend registers a distinct slice in the `store.config.ts` seam the create built for exactly this. **Gap:** `solution-authentication` and `solution-offline-sync` should also carry a `shared-state-project` `.extend` registering their `auth` / `notifications` slices (see [Findings](#findings) 4). |

**No `TMC`, no `FMC`, no `FDC`. Zero resolver solutions.** No `.create`-on-`.create` design error survives the pre-analysis retag. Every intersecting group is the design working: repo/composition-root buckets, or a constraint-ordered single-direction refine on a shared class.

## Findings

1. **`solution-ui-testing` spans two catalogs.** It `.create`s the monolith's `apps/component-preview` (`element/component-preview-project`) *and* `.extend`s the design-system repo's `projects/demo` (`element/design-system-repository`), and its V1 `depends_on` names both `design-system-structure`/`design-system-components` (design-system) and `repository-structure`/`forms` (monolith). This is the `TD-`/degenerate-split footnote from the skill: the code's own structure genuinely differs between the two sides (different preview app, different project targets). **Recommendation: split** into `solution-ui-testing` (monolith `ComponentTesting`, `depends_on` `repository-structure` + `app-testing` + `forms`) and `solution-design-system-ui-testing` (design-system `ComponentTesting`, `depends_on` `design-system-structure` + `design-system-components` + `app-testing`). Deferred — [tracked in DECISIONS.md](skills/angular/architecture/v3.1/agent/DECISIONS.md). Until then, both catalogs' `ComponentTesting` point at the single `solution-ui-testing` with the family-crossed `depends_on` noted.
2. **`element/repository` retag (done).** Recorded above; no further action.
3. **`element/ds-component-ts` retag (done).** The generic feature-lib `{component}.component.ts` (`component-name-component-ts`, extended by `forms` + `state-tiering`) is a separate `FMN` group — `forms` adds Signal Forms wiring, `state-tiering` adds a component `signal()` field. Both are common conventions, always co-present, member-disjoint. Canonical.
4. **`shared-state-project` slice-registration gap.** `store.config.ts` (from `solution-global-store`) is the seam where each slice-adding solution registers its reducer + effects. `solution-offline-first` carries this `.extend`; `solution-authentication` and `solution-offline-sync` currently register their slices only in prose. At Stage 4 (or a solution-update pass) each should carry an explicit `shared-state-project` `.extend`. Still `TMN` canonical once filled — each registers a distinct slice. This mirrors dotnet v3.1's `app-infrastructure-csproj` "created by the first persistence solution, extended by the rest" pattern.
5. **Cross-catalog element intersections are clean.** `platform-shell-project` is touched by monolith solutions and `solution-federation-host` (platform-host). Because a `platform-host` plateau composes a `monolith` plateau via `parent_plateaus`, these coexist only in a platform-host plateau — the registry entry for `platform-shell-project` at N≥3 lives there, `source: ordering-only` (federation host wiring registers after the monolith's own bootstrap wiring).

## Registry entries to create during plateau-create (Stage 4)

All canonical — every entry is a record + an ordering note, no resolver link.

| Element | Shallowest plateau | Ordering |
| --- | --- | --- |
| `feature-client-ts` | monolith `OfflineReadResilience` plateau (VP3+VP4) | `source: constraint` (VP4 requires VP3) |
| `feature-facade-ts` | monolith `OfflineWriteQueue` plateau (VP3+VP5) | `source: constraint` |
| `feature-routes-ts` | monolith `PerformanceTunedRouting` plateau (common+VP1) | `source: ordering-only` |
| `shared-state-project` | monolith first plateau with VP2 + any of VP4/VP5/VP7 | `source: constraint` (each requires VP2) |
| `component-name-component-ts` | monolith baseline (both `forms` + `state-tiering` common) | `source: ordering-only` |
| `monolith-repository`, `platform-shell-project` | monolith baseline + each VP that extends them | `source: ordering-only`, N≥3 note (benign) |
| `platform-contracts` | platform-host `SessionSharing` plateau | `source: constraint` (session-sharing depends_on platform-contracts) |
| `platform-shell-project` (with `federation-host`) | platform-host baseline plateau | `source: ordering-only`, N≥3 |
| `design-system-repository` | design-system single plateau | `source: ordering-only` |

## Out of scope

- **No fixed-point iteration needed** — zero resolvers built, so the grouping pass does not repeat.
- **Realized-by / solution names** follow the Stage-3 roster ([INVARIANTS §3](skills/angular/architecture/v3.1/agent/INVARIANTS.md)); the `solution-ui-testing` split (Finding 1) is the one outstanding structural change.
- **Registry files are not written here** — they are created per plateau during Stage 4, using [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/templates/registry-entry.template.md|registry-entry.template.md]].
