---
name: plateau-gw009-001--package-docs-integration-client
description: docs/integration/client/reputation — the consumed reputation contract of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when updating the reputation service's contract, adding a contract of another consumed service, or regenerating its client stubs
domain: skill
type: template
plateau: plateau-gw009-001
version: 20261012000000
tags:
  - skill/template/package
  - plateau/plateau-gw009-001
created_by:
  - "[[library/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]]"
---

# Goal
The reputation service's own contract, as its provider published it, from which this service's client stubs are generated.

__Applied solutions:__
- [[library/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[library/go/architecture/solutions/solution-external-integration.skill/Implementation/Repository.extend.md|Repository]]

# Core Principles
The file is the provider's: it is replaced wholesale, never edited. Its rules are those of `api-client-develop`.

# Structure
## Repository place
```
docs/
  integration/
    client/
      reputation/
```
## Package Structure
```
docs/integration/client/reputation/
  reputation.proto   ← the provider's published copy, unchanged
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| reputation.proto | `reputation.v1.ReputationService` with `CheckReputation`; carries the provider's `// version:` line and `go_package` | — |

# Allowed Dependencies
- None — the folder holds no Go file; its generated code is `gen/client/reputation/`.

# Rules
MUST:
- `reputation.proto` is byte-identical to the provider's published file; this module's Go import path is set by the managed mode of `buf.gen.client.yaml`, never by editing `go_package`.
- It is generated through `buf.gen.client.yaml` into `gen/client/reputation/` — never through `buf.gen.yaml`, never into `gen/api/`.
- Only `internal/infrastructure/reputationclient` imports `gen/client/reputation`.

__Applied solutions:__
- [[library/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[library/go/architecture/solutions/solution-external-integration.skill/Implementation/Repository.extend.md#MUST|Repository]]

# Check list
- [ ] `make proto-gen` regenerates `gen/client/reputation/` with no manual edit afterward.
- [ ] `make test-kind-generated` passes: `buf.gen.yaml` generates nothing from `docs/integration/client`.

__Applied solutions:__
- [[library/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[library/go/architecture/solutions/solution-external-integration.skill/Implementation/Repository.extend.md|Repository]]
