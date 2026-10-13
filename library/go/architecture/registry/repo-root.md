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
- [[library/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] (`.create`)
- [[library/testing/go/solution-conformance-testing-in-go.skill/solution-conformance-testing-in-go.skill.md|solution-conformance-testing-in-go]] (`.extend`)
- [[library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] (`.extend`)
- [[library/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] (`.extend`)
- [[library/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] (`.extend`)

`solution-cached-db` and `solution-persistent-db` contribute no `Repository` delta of their own — `internal/infrastructure/reputationcache` and `internal/infrastructure/linkstore` are ordinary new packages under the already-established `internal/infrastructure/` tree `solution-go-repository-structure` created, not a repo-root-level change.

# Classification
Paths in this entry's history are those of their time: until 2026-10-12 the own contract lay in `proto/linkcheck/` with `buf/buf.gen.yaml` and `gen/api/`, the consumed one in `proto/reputation/` with `buf/reputation.gen.yaml` and `gen/reputation/`. Since issue #206 they are `docs/integration/api/linkcheck/` → `gen/api/linkcheck/` through `buf.gen.yaml`, and `docs/integration/client/reputation/` → `gen/client/reputation/` through `buf.gen.client.yaml`, both modules of one root `buf.yaml`. The classification below is unchanged by the move; `buf.yaml` is a second file both solutions write — `grpc-api` creates it, `external-integration` adds a module.

`TDN` for the `{grpc-api, external-integration}` pair specifically, `FMN` for the rest. **T**: `solution-external-integration`'s own Rule explicitly requires it — "If `Makefile` already has a `proto-gen` target (from `solution-grpc-api`), add this generation as a second `buf generate` line inside the existing target — never a second `proto-gen:` target declaration" — a real, stated ordering dependency, confirmed by reading both `Repository.extend.md` files side by side. **D**: this is DI-substitution-shaped, not a code-logic change — `external-integration` extends the *recipe body* of an existing target rather than changing what the target conceptually does. **N**: independent — the two `buf generate` lines inside `proto-gen` operate on disjoint `proto/`/`gen/` subtrees (`proto/linkcheck`→`gen/api`, `proto/reputation`→`gen/reputation`) and neither reads the other's output. `solution-conformance-testing-in-go` stays `FMN` against all three others — its `Makefile` targets and `report-template/` remain untouched.

**GW009.001:** `solution-taskbox-in-go` adds `google/uuid` to `go.mod` and the `TEST_DATABASE_DSN` requirement of the test gate — `FMN`. `solution-go-db-migrations` adds `cmd/migrate` inside the established `cmd/` layout, no `Repository` delta.

# Ordering
`source: constraint` for `{grpc-api, external-integration}` — the ordering is stated in `external-integration`'s own Rule (quoted above), not merely a convention. `source: ordering-only` for everything else: `solution-go-repository-structure` must exist first (trivially, as this catalog's foundational solution); `solution-conformance-testing-in-go` has no ordering requirement relative to the others (disjoint target/file sets).

# Resolution
Canonical — no resolver needed. Verified by actually running `make proto-gen` (producing both `gen/api/*.go` and `gen/reputation/*.go`) and `make test-kind-unit` in the same checkout, at every plateau where this element's set changed.

# Architectural signal
N=4 at the deepest plateau, unchanged since `plateau-integrated-service`. The predicted collision between `grpc-api` and `external-integration` (first flagged at `plateau-dual-api-service`, N=3) happened exactly as anticipated and resolved exactly as `external-integration`'s own Rule already specified — a good example of the harness paying off: the N≥3 flag on an earlier plateau caused the next plateau's build to check the prediction deliberately instead of re-deriving it. This element has now stayed flat across two further plateaus (`plateau-cached-service`, `plateau-persistent-service`) while every other tracked element in this catalog grew — the expected, healthy shape for `repo-root`: it should only grow when a solution introduces genuinely new repository-level scaffolding, not merely a new package nested inside scaffolding that already exists.

# Growth history
| Plateau | N | What changed | Verified |
| --- | --- | --- | --- |
| `plateau-http-service` | 2 | First real: `solution-go-repository-structure` (create) + `solution-conformance-testing-in-go` | `Makefile` targets appended cleanly, disjoint from the base |
| `plateau-dual-api-service` | 3 | `solution-grpc-api` joins (`proto/`, `buf/`, `gen/api/`, `proto-gen` target); crosses N≥3, flagged as worth re-checking once `external-integration` also touches `proto-gen` | `make proto-gen` + `make test-kind-unit` both clean in the same checkout |
| `plateau-integrated-service` | 4 | `solution-external-integration` joins — the predicted `proto-gen` collision with `grpc-api` happens exactly as anticipated, resolved exactly as `external-integration`'s own Rule specifies (second `buf generate` line inside the existing target) | `make proto-gen` regenerates both `gen/api/*.go` and `gen/reputation/*.go` correctly |
| `plateau-cached-service` | 4 | Unchanged — `solution-cached-db` contributes no `Repository` delta | Unchanged behavior reconfirmed |
| `plateau-persistent-service` | 4 | Unchanged — `solution-persistent-db` contributes no `Repository` delta | `make proto-gen` regenerates both outputs including the new `RecentChecks` RPC with no manual edits needed |
| `gw009-001` (GW009.001) | 5 | `solution-taskbox-in-go` joins (`go.mod`, `TEST_DATABASE_DSN` for `make test-kind-unit`) | `TEST_DATABASE_DSN=… make test-kind-unit` green (48 tests) |
| every plateau with VP1 (2026-10-12, issue #206) | unchanged | The contract layout moves under `docs/integration/`; `buf.yaml` and the `generated` test kind join `solution-grpc-api`'s delta; `external-integration` adds the second buf module and `buf.gen.client.yaml` | `make proto-gen` and `make test-kind-generated` green with both modules present |
