---
name: registry-repo-root
description: Conflict Detection result for the `repo-root` element
tags:
  - concern/architecture
  - stack/go
  - element/repo-root
---

# Element
`repo-root`

# Involved solutions
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] (`.create`)
- [[skills/go/architecture/solutions/solution-go-conformance-testing.skill/solution-go-conformance-testing.skill.md|solution-go-conformance-testing]] (`.extend`)
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] (`.extend`) — new at this plateau, adds `proto/`, `buf/`, `gen/api/`, and the `Makefile`'s `proto-gen` target

# Classification
`FMN` — **F**: no Constraint. **M**: each extending solution adds disjoint `Makefile` targets and/or disjoint new files/directories. **N**: independent — `solution-go-conformance-testing` adds `unit-test`/`mutation-test`/`test-report`/`test-and-report` + `report-template/`; `solution-grpc-api` adds `proto-gen` + `proto/`/`buf/`/`gen/api/`. No target name or file path collides between the two extending solutions.

# Ordering
`source: ordering-only` — `solution-go-repository-structure` must exist first (there is no `Makefile` to extend otherwise); trivially satisfied since it is this catalog's foundational solution. `solution-go-conformance-testing` and `solution-grpc-api` have no ordering requirement relative to each other — verified by their disjoint target/file sets.

# Resolution
Canonical — no resolver needed. New targets appended to an existing `Makefile`, and new standalone files/directories, do not collide with anything the other solutions created — verified by actually running `make proto-gen` and `make unit-test` in the same checkout.

# Architectural signal
N=3 at this plateau, up from N=2 at `plateau-http-service` — crosses the N≥3 threshold for the first time on this element. Unlike [[./cmd-service-main-go.md|cmd-service-main-go]], every contributor here still adds strictly disjoint content (no restructuring of another solution's output), so the growing count is not yet a real risk signal on its own — worth re-checking once [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] (also `Repository.extend`s for its own `proto/{external}/`) is composed at `plateau-integrated-service`, since its own Rule explicitly anticipates sharing `Makefile`'s `proto-gen` target with `solution-grpc-api`.
