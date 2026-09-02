# Angular architecture v3.1

Rebuild of the V1 Angular architecture catalog through the plateau-map pipeline
(`feature-map-create` → `variability-map-create` → solution migration →
`delta-conflict-detection` → `plateau-create-by-solutions`), mirroring the
`skills/dotnet/architecture/v3.1` effort.

## Three catalogs

The V1 catalog spans products that are cardinally different — different
deployment topology, different repositories, a monolith→platform migration heavy
enough that one shared variability space would force every reader to filter half
the rows. v3.1 splits them into **three catalogs**, each with its own
`feature/`, `variability-map.md`, and `plateau/`:

| Catalog | Product | Composes | V1 plateaus it replaces |
| --- | --- | --- | --- |
| [`monolith/`](monolith/feature/feature-model.md) | A single-deployable Angular web application | — | online-monolith, async-monolith, offline-monolith, monitored-app, multiuser-app |
| [`platform/`](platform/feature/feature-model.md) | A federation system: one host + N independently-deployed remotes + `@platform/contracts` | `monolith/` (the host **is** a monolith + federation, via `parent_plateaus`) | platform-monolith, embeddable-app |
| [`design-system/`](design-system/feature/feature-model.md) | An independently versioned component-library npm package | — | design-system |

- `platform/` plateaus set `parent_plateaus` to the corresponding `monolith/`
  plateaus, so the monolith's feature content is **composed, not duplicated**.
  `platform/`'s own variability space is only the federation delta (host
  federation, contracts, the remote contract, session sharing).
- `design-system/` has no dependency on the other two; both consume it (as a
  plain npm dependency in `monolith/`, upgraded to a version-negotiated
  federation singleton in `platform/`).
- **One shared solution pool**: `v3.1/solutions/` holds every migrated V1
  solution once. Each catalog's variability-map and plateaus link into it. A few
  V1 solutions are two-sided (host vs remote, platform vs design-system target)
  and are split into separate solutions during `delta-conflict-detection`.

## Layout

```
v3.1/
  README.md                     ← this file
  agent/
    DECISIONS.md                ← build decisions log + open questions
    INVARIANTS.md               ← anchor contract for the solution migration (added at Stage 3)
    check.sh                    ← mechanical checks (added at Stage 3)
  monolith/  platform/  design-system/
    feature/
      feature-model.md
      diagrams/feature-diagram.mmd
    variability-map.md          ← Stage 2
    plateau/                    ← Stage 4
  solutions/                    ← Stage 3 (shared pool)
```

## Status

- **Stage 1 (feature-map-create)** — in progress. Three feature models drafted.
- Stages 2–4 — pending.

V1 input (read-only reference): `skills/angular/architecture/solutions/`,
`skills/angular/architecture/plateau/`.
