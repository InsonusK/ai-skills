---
name: feature-lib-split-conditional-on-backend-data-access
description: Whether every feature always gets a data-access lib, or only features that talk to a backend
problem: V1 mandated a `feature` + `data-access` split for every feature "from the start"; once BackendDataAccess is a Variation Point, a no-backend feature has no data-access lib
decision: A feature always has a `feature` lib; the `data-access` lib is added by solution-api-http-layer only when the feature has server data (BackendDataAccess = Yes)
tags:
  - solution/repository-structure
  - stack/typescript
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem

V1's `solution-repository-structure` mandates: *"every business feature is split into at least a `feature` lib and a `data-access` lib **from the start**"*. V1 could say this because every V1 plateau had backend data access — there was no such thing as a feature without a server.

v3.1 makes **BackendDataAccess a Variation Point** ([monolith VP3](skills/angular/architecture/v3.1/monolith/variability-map.md)): a plateau can answer `BackendDataAccess = No` (a local-only app), and even in a `BackendDataAccess = Yes` plateau an individual feature may have no server data (a purely client-side tool, a settings panel backed only by `PersistedState`). For such a feature a `data-access` lib would be an empty project — a `type:data-access` tag, an `index.ts`, a `project.json`, and nothing to put in them.

The question: does `solution-repository-structure` still create a `data-access` lib for every feature, or does it become conditional?

# Selected variant

**Selected variant:** [[#data-access lib is conditional on BackendDataAccess]]

`solution-repository-structure` creates only the `feature` lib for a feature. `solution-api-http-layer` (the realization of `BackendDataAccess = Yes`) adds the `data-access` lib, its Facade/Client/Mapper shape, and the `feature → data-access` boundary allow-list row. A feature with no server data has just the `feature` lib and never a hollow `data-access` project.

# Searched variants

## Always create both libs

### Description

Keep the V1 rule unchanged — `solution-repository-structure` scaffolds `libs/{feature}/feature` **and** `libs/{feature}/data-access` for every feature, regardless of whether the feature has a backend.

### Benefits

- One rule, no conditionality — every feature folder looks identical.
- A feature that later gains server data already has the lib.
- The `feature → data-access` boundary allow-list row is always present.

### Costs

- A no-backend feature ships an empty `data-access` project: a `type:data-access` tag, `project.json`, `index.ts`, a test target — all with nothing in them. `nx graph` and `nx affected` carry a dead node.
- Contradicts the VP: `BackendDataAccess` is supposed to be the thing that *introduces* the data layer. If the lib exists unconditionally, the VP only toggles whether it has content — a weaker, fuzzier boundary.
- `solution-repository-structure` would have to know about `data-access`'s internal shape (Facade/Client/Mapper) to scaffold it meaningfully, coupling the base structure to the HTTP-layer solution.

## data-access lib is conditional on BackendDataAccess (selected)

### Description

`solution-repository-structure` creates only `libs/{feature}/feature` for a feature (components, routing, feature Signal Store). `solution-api-http-layer` — the solution that realizes `BackendDataAccess = Yes` — adds `libs/{feature}/data-access` with its Facade/Client/Mapper/Errors layering and the `feature → data-access` (same scope) boundary allow-list row. A feature with no server data has just the `feature` lib.

### Benefits

- The VP means what it says: `BackendDataAccess = Yes` is exactly the point at which a feature's data layer appears.
- No empty projects — every `data-access` lib that exists has real content.
- `solution-repository-structure` stays the minimal base: `apps/platform-shell` + `libs/shared/{ui,util}` + a `feature` lib per feature. It knows nothing about Facade/Client/Mapper — that is `solution-api-http-layer`'s concern.
- A no-backend plateau (`BackendDataAccess = No`) has a clean, honest structure with no vestigial data-access tag.

### Costs

- Two solutions now shape a feature's lib set instead of one — a reader must know that `data-access` comes from `solution-api-http-layer`, not the base structure. Mitigated by this ADR, the `Repository.create` note, and the variability-map row.
- A feature that starts with no backend and later gains one needs the `data-access` lib added retroactively. This is the same "split later" cost the always-both variant avoids — but it only applies to a genuinely rare transition, and `solution-api-http-layer`'s own `Repository.extend` is the documented way to do it.
- Every monolith plateau built so far answers `BackendDataAccess = Yes` with every feature having server data, so the split is present in all of them — the conditionality is currently only visible in the solution text, not in an example. Acceptable: the VP and the ADR record the intent for a thinner plateau a future consumer may build.
