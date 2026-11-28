---
name: app-domain-layer
description: Entities and domain services — invariant ownership, public construction and behavior, and reusable business logic without transport or integration mechanics
whenToUse: when adding or reviewing a domain entity, its construction or state changes, or business logic shared by several commands
updated: 20261128
tags:
  - stack
  - concern/architecture
  - concern/coding
  - app-type/service
  - app-type/cli
---

# Goal
- Business invariants have one owner and remain enforced through every caller.
- Entities and domain services have distinct responsibilities.

# Core Principle
- **Domain boundaries** - Apply [app-layers](./app-layers.skill/app-layers.skill.md) for permitted dependencies and the distinction between domain services and integration services.
- **Behavior ownership** - An entity owns rules of one domain object; a domain service owns reusable business logic that coordinates domain objects and allowed service contracts.

# Rule

## MUST

### One business rule, one method
Keep each business rule in exactly one entity or domain-service method and call that method from every command that needs it.
- Violation: "a user name is unique" checked in `AddUser` and again, differently, in `RenameUser`.
- Risk: the copies drift, and the same event is handled differently depending on the path.
- Fix: move the rule into one domain-service method and call it from both commands.

### Keep construction and mutation behind domain APIs
Expose entity construction and state changes through public domain APIs that enforce the entity's invariants.
- Risk: An integration mapper or command bypasses validation and creates an invalid entity.
- Fix: Keep validation and behavior in the entity and have callers use those APIs rather than copy invariants into DTO mapping.

### Test domain behavior without transport or engine dependencies
Test entities directly and domain services with service boundaries or allowed own-store repository contracts replaced by fakes.
- Risk: Domain tests depend on a remote API or database and obscure the business rule under test.
- Fix: Exercise public construction and behavior, including invariant failures; integration tests belong to the mechanism owning the actual I/O.

# Check list
- [ ] Check [business rule ownership](#one-business-rule-one-method).
- [ ] Check [construction and mutation](#keep-construction-and-mutation-behind-domain-apis).
- [ ] Check [domain tests](#test-domain-behavior-without-transport-or-engine-dependencies).
