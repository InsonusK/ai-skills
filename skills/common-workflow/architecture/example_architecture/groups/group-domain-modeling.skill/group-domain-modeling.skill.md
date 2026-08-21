---
name: group-domain-modeling
description: Owns invariant ownership and value semantics for this module — where a validation condition is written first, and when it moves to a shared, reusable shape
whenToUse: when adding a new value-object-shaped property, a new entity state-mutation method, or when the same validation condition is about to be written in a second place
domain: skill
type: solution-group
version: 20260821
tags:
  - skill/architecture/solution-group
  - stack/dotnet
  - concern/architecture
  - group/domain-modeling

contains:
  - "[[../../solutions/solution-value-object.skill/solution-value-object.skill.md|solution-value-object]]"
  - "[[../../solutions/solution-entity-invariant.skill/solution-entity-invariant.skill.md|solution-entity-invariant]]"
  - "[[../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]]"
  - "[[../../solutions/solution-condition-ownership.skill/solution-condition-ownership.skill.md|solution-condition-ownership]]"
depends_on:
  - "[[../../solutions/solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]]"
---

# Goal
- Give every value-object and entity-invariant condition exactly one place it is first written: local to the type that owns it.
- Let a condition move to a shared, reusable shape once — and only once — it turns out to be needed by a second owner, without that move being a separate, hand-invented decision every time it comes up.

# Core Principle
- A condition starts local. [[../../solutions/solution-value-object.skill/solution-value-object.skill.md|solution-value-object]] and [[../../solutions/solution-entity-invariant.skill/solution-entity-invariant.skill.md|solution-entity-invariant]] each work completely on their own, with no dependency on a shared rules abstraction.
- [[../../solutions/solution-condition-ownership.skill/solution-condition-ownership.skill.md|solution-condition-ownership]] is this group's dispatcher: it is the one place that decides whether a condition stays local or moves into [[../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]].
- This group `depends_on` nothing except the shared project foundation — every solution inside it is self-sufficient by design; only the *decision* solution depends on its own siblings.

# Rule

## MUST
- Apply `solution-domain-rule` only after `solution-condition-ownership` has found the same condition duplicated across two or more owners — never speculatively.
  - Risk: centralizing early produces a shared abstraction with exactly one caller, which is pure indirection with no de-duplication benefit yet.
  - Fix: keep the condition local until a second owner genuinely needs it; let `solution-condition-ownership` make that call explicit.

# Check list
- [ ] Every value-object property and every entity mutation method has a local, first-written condition before `solution-condition-ownership` is ever consulted.
- [ ] `solution-domain-rule` is applied only for a condition `solution-condition-ownership` has actually found duplicated across two or more owners.
- [ ] No solution outside this group is listed in this group's `depends_on`.
