---
description: The store-independent TaskBox package — task values, handler registry, classification, worker
name: internal/taskbox
element_kind: package
change_kind: create
tags:
  - solution/taskbox-in-go
  - element/internal-taskbox
---

# Goals
- One place holding the TaskBox mechanism every store shares, so a service using two stores (PostgreSQL for Critical tasks, Redis for NonCritical ones) runs one worker type and one handler registry.

# Core Principles
- `internal/taskbox` is a **shared mechanism**, not an adapter: store adapters (`internal/infrastructure/*`) import it to enqueue inside their own transactions, which the "no sibling infrastructure import" rule would forbid if it lived under `internal/infrastructure/` — see [[../../../adr/taskbox-package-outside-infrastructure.md|adr/taskbox-package-outside-infrastructure]].
- Each store is its own subpackage (`pgstore`, later `redisstore`), so a binary links only the drivers it uses.

# Structure

## Repository place
```
internal/
  taskbox/
```

## Package Structure
```
internal/taskbox/
  taskbox.go
  handler.go
  worker.go
  pgstore/
  features/
    taskbox-conformance.feature
  test/
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| taskbox.go | task values, `Store` interface | [[./taskbox.go.create.md|taskbox.go]] |
| handler.go | `Handler`, `Registry`, `Retryable`, `Backoff` | [[./handler.go.create.md|handler.go]] |
| worker.go | `Worker` pool | [[./worker.go.create.md|worker.go]] |
| pgstore/ | PostgreSQL store | [[./pgstore/Package.create.md|pgstore]] |
| features/, test/ | the conformance feature and its runner | [[./test/Package.create.md|test]] |

# Go Dependencies
| Module | Version constraint | Purpose |
| ------ | ------------------- | ------- |
| github.com/google/uuid | >= 1.6 | `uuid.NewV7` task ids (contract §1) |

# What Does NOT Belong Here
- Task handlers — they are inbound adapters in [[../api/tasks/Package.create.md|internal/api/tasks]].
- Any business rule about *when* a task is needed — that is the domain service's decision.
- Schema DDL — it is a migration of the service ([[../infrastructure/{store}/Package.extend.md|{store} migrations]]).

# Allowed Dependencies
- Standard library, `github.com/google/uuid`; the store subpackages add their own driver.
- Never `internal/domain/*` or `internal/infrastructure/*` — the mechanism knows nothing about the service.

# Rules

## MUST

### Keep the mechanism free of the service
Never import a domain or infrastructure package from `internal/taskbox` or its subpackages.
- Risk: the mechanism can no longer be copied between services, and an import cycle appears as soon as an adapter enqueues through it.
- Fix: the service reaches TaskBox through `NewTask`, `Registry`, and the store; TaskBox never reaches back.

# Check list
- [ ] `internal/taskbox` imports only the standard library and `google/uuid`; `pgstore` adds only `pgx`.
- [ ] No handler and no business rule lives under `internal/taskbox`.
