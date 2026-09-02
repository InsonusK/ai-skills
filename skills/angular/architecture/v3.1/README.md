# Angular architecture v3.1

Rebuild of the V1 Angular architecture catalog through the plateau-map pipeline
(`feature-map-create` → `variability-map-create` → solution migration →
`delta-conflict-detection` → `plateau-create-by-solutions`), mirroring the
`skills/dotnet/architecture/v3.1` effort.

## Four catalogs

The V1 catalog spans products that are cardinally different — different
deployment topology, different repositories, different workspace tooling. Each
gets its own catalog with its own **single concrete baseline**, `feature/`,
`variability-map.md`, and `plateau/`:

| Catalog | Product | Baseline | Composes | V1 plateaus it replaces |
| --- | --- | --- | --- | --- |
| [`monolith/`](monolith/feature/feature-model.md) | A single-deployable Angular web application | Nx workspace: shell + feature libs + routing + testing | — | online-monolith, async-monolith, offline-monolith, monitored-app, multiuser-app |
| [`platform-host/`](platform-host/feature/feature-model.md) | The **host** of a federation system + the `@platform/contracts` package | a `monolith/` + federation host config + contracts package | `monolith/` (a host **is** a monolith, via `parent_plateaus`) | platform-monolith |
| [`embeddable-app/`](embeddable-app/feature/feature-model.md) | A **remote** — an independently deployed app a host loads at runtime | minimal: `remoteEntry` + exposed module + shared singletons (any tooling) | — (optionally a `monolith/` if `RemoteInternalArchitecture`) | embeddable-app |
| [`design-system/`](design-system/feature/feature-model.md) | An independently versioned component-library npm package | Angular CLI multi-project: library + demo | — | design-system |

- **`platform-host/` composes `monolith/`**: its plateaus set `parent_plateaus`
  to the corresponding `monolith/` plateaus, so the monolith's feature content is
  **composed, not duplicated**. `platform-host/`'s own variability space is only
  the federation delta (dynamic remote federation, the contracts package,
  design-system-as-federation-singleton, session sharing, federated read
  resilience).
- **`embeddable-app/` and `platform-host/` are two roles in one distributed
  product** but have incompatible baselines (Nx monolith vs. any-tooling
  contract-conformant), so they are separate catalogs. They reference each other
  by cross-catalog `Requires`, never `parent_plateaus`.
- **`design-system/`** has no dependency on any other catalog; all three others
  consume its published package.
- **One shared solution pool**: `v3.1/solutions/` holds every migrated V1
  solution once. Each catalog's variability-map and plateaus link into it. A few
  V1 solutions are two-sided (host vs. remote, platform vs. design-system target)
  and are split into separate solutions during `delta-conflict-detection`.

## How to read this: feature-map → variability-map → plateau

1. **feature-map** (`{catalog}/feature/feature-model.md`) — the *capability
   space*: which features exist, which are **common** (in every member of that
   catalog's family), which are **variable** (a Variation Point — an axis two
   teams could answer differently).
2. **variability-map** (`{catalog}/variability-map.md`, Stage 2) — turns each
   variable feature into a VP row: variants, constraints (`requires`), and the
   solution(s) that realize it.
3. **plateau** (`{catalog}/plateau/*`, Stage 4) — one *named point* in the
   VP-combination space: a fixed set of Yes/No answers. E.g. a `monolith/`
   plateau `online-monolith` ≈ `{BackendDataAccess=Yes, GlobalStore=No,
   Offline=No, Auth=No}`.

## Layout

```
v3.1/
  README.md                     ← this file
  agent/
    DECISIONS.md                ← build decisions log + open questions
    INVARIANTS.md               ← anchor contract for solution migration (Stage 3)
    check.sh                    ← mechanical checks (Stage 3)
  monolith/  platform-host/  embeddable-app/  design-system/
    feature/
      feature-model.md
      diagrams/feature-diagram.mmd
    variability-map.md          ← Stage 2
    plateau/                    ← Stage 4
  solutions/                    ← Stage 3 (shared pool)
```

## Status

- **Stage 1 (feature-map-create)** — in progress. Four feature models drafted.
- Stages 2–4 — pending.

V1 input (read-only reference): `skills/angular/architecture/solutions/`,
`skills/angular/architecture/plateau/`.
