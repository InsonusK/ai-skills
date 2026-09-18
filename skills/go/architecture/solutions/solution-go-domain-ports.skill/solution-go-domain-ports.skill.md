---
name: solution-go-domain-ports
description: Shared prerequisite that creates the internal/domain/interfaces package the first time any solution needs to declare an outbound port
whenToUse: when a solution needs to declare the module's first outbound port (an interface the domain depends on and an infrastructure adapter implements) and internal/domain/interfaces does not exist yet
domain: skill
type: architecture
version: 20260917000000
tags:
  - skill/architecture/solution
  - solution/go-domain-ports
  - stack/go
  - concern/architecture
creates:
  - "internal/domain/interfaces/"
extends:
depends_on:
built_on_plateau:
adr:
  - "[[skills/go/architecture/solutions/solution-go-domain-ports.skill/adr/shared-prerequisite-not-common-baseline.md|Shared prerequisite, not part of the common baseline]]"
---

# Goal
- Give the first solution that needs an outbound port a package to declare it in, without that solution — or any sibling solution needing a port of its own — re-deciding the package's own doc comment, name, or place in the tree.

# Capabilities
- Every outbound-port-needing solution (`solution-external-integration`, `solution-cached-db`, `solution-persistent-db`, and any future one) shares one `internal/domain/interfaces` package instead of each risking a double-create if a plateau composes more than one of them.

# Core Principles
- This package declares interfaces only — no implementation, no reference to any concrete infrastructure technology.
- Every interface here is named for what the domain needs, never for the technology that will implement it.

# Boundaries
- This solution creates the package itself but declares no interface in it — the first solution applied on top (`solution-external-integration`, `solution-cached-db`, or `solution-persistent-db`) adds its own port as a new file in this package.

# Adr
- [[skills/go/architecture/solutions/solution-go-domain-ports.skill/adr/shared-prerequisite-not-common-baseline.md|Shared prerequisite, not part of the common baseline]]
  - Selected variant: a shared prerequisite three VP-realizing solutions `depends_on`, not a common-baseline solution every plateau applies

# Template Skill Mutations
FILES:
- [[./Implementation/internal/domain/interfaces/Package.create.md|internal/domain/interfaces]] - create - empty ports package, doc comment only

# Workflow

## First port-needing solution applies
1. A plateau composes `solution-external-integration` (or `-cached-db`/`-persistent-db`) for the first time.
2. That solution's `depends_on` brings in `solution-go-domain-ports`, which creates `internal/domain/interfaces/` with its package doc comment and nothing else.
3. The applying solution then adds its own interface as a new file in that package.

## Second port-needing solution applies to the same plateau
1. A plateau already has `internal/domain/interfaces/` (from step above) and composes a second port-needing solution.
2. `solution-go-domain-ports` is already satisfied (the package exists) — the second solution only adds its own new file; it never re-creates the package.

# Rules

## MUST
- [[./Implementation/internal/domain/interfaces/Package.create.md#MUST|internal/domain/interfaces]]

# Check list
- [ ] `internal/domain/interfaces/` exists with a package doc comment and no interface declared by this solution itself.
- [ ] No two applied solutions both `.create` this package — the second and later port-needing solutions only add a new file to it.
