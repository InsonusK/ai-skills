---
name: group-request-handling
description: Owns the request pipeline for this module — how a client-facing intent becomes a Command, gets transport-validated, and reaches a domain method
whenToUse: when adding a new write operation (Command + Handler) for this module, or deciding where a new input-shape check belongs
domain: skill
type: solution-group
version: 20260821
tags:
  - skill/architecture/solution-group
  - stack/dotnet
  - concern/architecture
  - group/request-handling

contains:
  - "[[../../solutions/solution-command-handler.skill/solution-command-handler.skill.md|solution-command-handler]]"
  - "[[../../solutions/solution-transport-validation.skill/solution-transport-validation.skill.md|solution-transport-validation]]"
depends_on:
  - "[[../../solutions/solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]]"
  - "[[../group-domain-modeling.skill/group-domain-modeling.skill.md|group-domain-modeling]]"
adr:
  - "[[./adr/depends-on-domain-modeling-as-group.md|Depend on domain-modeling as a group, not per-solution]]"
---

# Goal
- Give every write operation on this module the same shape: a Command, a Handler that loads state and calls exactly one guarded domain method, and a transport validator that rejects malformed input before the handler runs.
- Keep this group's dependency on domain concepts (value objects, entity methods) at exactly one declared edge, instead of one edge per domain solution a handler happens to touch.

# Core Principle
- Every handler in this group eventually calls into [[../group-domain-modeling.skill/group-domain-modeling.skill.md|group-domain-modeling]] — it loads an entity and calls one of its guarded methods. Which specific domain solution (value object, entity invariant, or the centralized rule) it ends up touching is an implementation detail of the handler being written, not something this group's structure should hard-code.
- [[./adr/depends-on-domain-modeling-as-group.md|This group therefore depends on `group-domain-modeling` as a whole]], not on `solution-value-object`/`solution-entity-invariant` individually.

# Adr
- [[./adr/depends-on-domain-modeling-as-group.md|Depend on domain-modeling as a group, not per-solution]]
  - Selected variant: one group-level `depends_on` edge

# Rule

## MUST
- Declare `depends_on` on `group-domain-modeling` at the group level, never re-declare it on `solution-command-handler` or `solution-transport-validation` individually.
  - Risk: a later solution added to this group could silently omit the edge, or a reviewer could add it pointing at the wrong individual domain solution — both mistakes the group-level edge makes structurally impossible.
  - Fix: keep the single group-level edge; see [[./adr/depends-on-domain-modeling-as-group.md|the ADR]] for the rejected per-solution alternative.

# Check list
- [ ] `group-domain-modeling` appears in this group's own `depends_on` exactly once.
- [ ] Neither `solution-command-handler` nor `solution-transport-validation` repeats that dependency in its own frontmatter.
