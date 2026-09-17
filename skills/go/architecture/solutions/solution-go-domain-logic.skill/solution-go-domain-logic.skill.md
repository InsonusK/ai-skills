---
name: solution-go-domain-logic
description: The domain layer — business logic in internal/domain/services, depending on infrastructure only through outbound ports it declares itself, never the reverse
whenToUse: when adding the first piece of business logic to a Go web-service, or reviewing whether a change put transport/infrastructure-specific code where domain logic should be
domain: skill
type: architecture
version: 20260917000000
tags:
  - skill/architecture/solution
  - solution/go-domain-logic
  - stack/go
  - concern/architecture
creates:
  - "internal/domain/services/"
extends:
depends_on:
built_on_plateau:
adr:
---

# Goal
- Give the module a business-logic layer that is transport-agnostic (no HTTP/gRPC type ever appears in it) and, at this solution's own baseline, dependency-free (no outbound port either, since nothing external exists yet to abstract over).
- Establish the one asymmetry every later solution in this catalog relies on: outbound dependencies (infrastructure) are declared as interfaces in `internal/domain/interfaces`; inbound adapters (transport) call the domain service's concrete type directly.

# Capabilities
- One place — `internal/domain/services` — a reader goes to find out what the module actually does, independent of how it is reached (HTTP, gRPC, ...) or what it is backed by (nothing, Redis, Postgres, an external service).
- A domain service that stays unit-testable with zero fakes at this solution's baseline, since it has no dependency to fake yet.

# Core Principles
- Inbound adapters depend on the domain service's *concrete* type; only outbound dependencies are interfaces — an inbound port interface here would have exactly one implementation ever, so it buys nothing an adapter calling the concrete type directly doesn't already have.
- A domain service's constructor takes only the ports (interfaces) it needs, never a concrete infrastructure type — enforced structurally later, once `internal/domain/interfaces` exists, by there being nothing else importable from `internal/infrastructure`.
- Every exported method's first parameter is `context.Context`, even when this solution's own baseline has no use for it yet.

# Boundaries
- This solution's own illustrative service has no outbound port because nothing in the common baseline needs one. The moment a real capability needs infrastructure, that capability's own solution ([[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]], [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]], or [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]) extends this service's constructor with the port it needs — this solution does not anticipate which one will come first.

# Template Skill Mutations
FILES:
- [[./Implementation/internal/domain/services/Package.create.md|internal/domain/services]] - create - the domain-services package: goal, allowed dependencies, what does not belong here
- [[./Implementation/internal/domain/services/{service}.go.create.md|{service}.go]] - create - one illustrative domain service, zero dependencies at this baseline

# Workflow

## Call a domain method (happy path)
1. An inbound adapter (created by whichever API solution is applied) calls `{Service}.{Method}(ctx, args)` on the concrete service instance `main.go` constructed.
2. The method validates its arguments and returns a result or a domain-level error — no transport type is constructed or referenced.
3. The adapter translates the result (or error) into its own transport's shape.

# Rules

## MUST
- [[./Implementation/internal/domain/services/Package.create.md#MUST|internal/domain/services]]
- [[./Implementation/internal/domain/services/{service}.go.create.md#MUST|{service}.go]]

# Check list
- [ ] `internal/domain/services` imports nothing under `internal/api` or `internal/infrastructure`.
- [ ] The illustrative service's constructor takes zero arguments at this solution's baseline.
- [ ] Every exported method's first parameter is `context.Context`.
