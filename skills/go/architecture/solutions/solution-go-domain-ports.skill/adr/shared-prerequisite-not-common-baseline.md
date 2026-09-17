---
name: shared-prerequisite-not-common-baseline
description: Whether internal/domain/interfaces is a common-baseline package or a shared prerequisite three VP-realizing solutions depend on
problem: Three independent VP-realizing solutions (ExternalIntegration, CachedDb, PersistentDb) each need to declare an outbound port the first time they are applied. If each created internal/domain/interfaces itself, a plateau composing more than one of them would double-create the same package.
decision: A separate shared-prerequisite solution (solution-go-domain-ports) creates the empty package; the three port-needing solutions depend_on it and only add their own file.
tags:
  - solution/go-domain-ports
  - concern/documentation
  - concern/documentation/adr
  - stack/go
---

# Problem

`solution-external-integration`, `solution-cached-db`, and `solution-persistent-db` each need `internal/domain/interfaces` to exist so they can declare their own outbound port there. The Feature Model's baseline explicitly has no such package — it is created only once the first port-needing feature is selected (`feature/feature-model.md`'s baseline note). A plateau is free to compose any subset of these three VPs, including two or three of them together (e.g. this catalog's own `plateau-cached-service`, which composes `CachedDb` on top of a plateau that does not yet have `PersistentDb`, and `plateau-persistent-service`, which has both). Whichever solution is responsible for creating the package must not collide with another solution doing the same thing on the same plateau.

# Selected variant

**Selected variant:** [[#Shared prerequisite solution, depended on by every port-needing solution (selected)]]

# Searched variants

## Shared prerequisite solution, depended on by every port-needing solution (selected)

### Description

Add `solution-go-domain-ports`, whose only Implementation file `.create`s the empty `internal/domain/interfaces` package (doc comment, no interface). `solution-external-integration`, `solution-cached-db`, and `solution-persistent-db` each add it to `depends_on` and add only their own new file to the package — never the package itself.

### Benefits

- Exactly one solution owns the package's creation; delta-conflict-detection has nothing to resolve, since every port-needing solution's file-level `element/*` tag is distinct (a different port file each).
- Mirrors the dotnet catalog's own `solution-api-project` shared-prerequisite pattern for the same shape of problem (two solutions — HTTP and gRPC publication — both needing `{Module}.Api.csproj` to exist first).
- Keeps the Feature Model's stated baseline (no `internal/domain/interfaces` until a port-needing feature is selected) intact — this solution is not part of the common baseline, only a dependency of the three VPs that need it.

### Costs

- A 13th catalog solution whose own Implementation is a single near-empty file.
- Every port-needing solution's `depends_on` must be kept in sync if a future fourth port-needing VP is added.

## Each port-needing solution creates the package for itself, guarded by "if not already present"

### Description

Let `solution-external-integration`, `solution-cached-db`, and `solution-persistent-db` each `.create` `internal/domain/interfaces` on their own, with prose telling the applying agent to skip creation if the package already exists.

### Benefits

- No extra solution to maintain.

### Costs

- "Create, unless it already exists" is exactly the double-create shape [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]] treats as a design error, pushed into prose instead of being resolved structurally — the agent applying a second port-needing solution has to notice and skip a step instead of the solution set making that impossible by construction.
- Three near-identical copies of the same package doc comment and Boundaries prose to keep in sync.

## Fold internal/domain/interfaces into the common baseline (created by solution-go-repository-structure)

### Description

Have the repository-structure solution create the empty `internal/domain/interfaces` package unconditionally, so it always exists regardless of which VPs a plateau selects.

### Benefits

- No shared-prerequisite solution needed at all; one fewer `depends_on` edge everywhere.

### Costs

- Contradicts the Feature Model's own stated baseline (`feature/feature-model.md`: "No `internal/domain/interfaces/` exists at this baseline"), which was itself confirmed against the concrete baseline-structure test this catalog's Feature Model stage requires.
- `plateau-http-service` and `plateau-dual-api-service` (this build's first two plateaus) would ship a permanently-empty package with nothing in the catalog ever depending on it existing — dead structure in the two plateaus most likely to be used as a genuinely minimal starting point.
