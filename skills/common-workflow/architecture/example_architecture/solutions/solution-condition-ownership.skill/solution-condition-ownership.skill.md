---
name: solution-condition-ownership
description: Decides whether a validation condition stays local to its first owner or moves into solution-domain-rule, based on whether a second owner now needs the same condition
whenToUse: when the same condition (e.g. an email-format check) is about to be written in a second place — one owner already has it locally and another now needs it too
domain: skill
type: architecture
kind: decision
group: "[[../../groups/group-domain-modeling.skill/group-domain-modeling.skill.md|group-domain-modeling]]"
version: 20260821
tags:
  - skill/architecture/solution
  - solution/condition-ownership
  - stack/dotnet
  - concern/architecture
creates:
extends:
depends_on:
  - "[[../solution-value-object.skill/solution-value-object.skill.md|solution-value-object]]"
  - "[[../solution-entity-invariant.skill/solution-entity-invariant.skill.md|solution-entity-invariant]]"
  - "[[../solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]]"
adr:
  - "[[./adr/local-vs-centralized-condition.md|Local-first, centralize on second owner]]"
---

# Goal
- Give an agent one explicit place to decide "does this condition stay local, or does it move into `EmailRule`" instead of that call being made ad hoc, differently, each time it comes up.
- Keep centralization lazy: a condition is never moved into `solution-domain-rule` before a real second owner exists.

# Core Principle
- Count the current owners of the condition. One owner → do nothing beyond [[../solution-value-object.skill/solution-value-object.skill.md|solution-value-object]]/[[../solution-entity-invariant.skill/solution-entity-invariant.skill.md|solution-entity-invariant]] already having it locally. Two or more owners → apply [[../solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] and redirect every owner to it.
- This solution produces no code of its own — see [[./Implementation/decision-table.md|Implementation/decision-table.md]] for what each branch actually produces.
- [[./adr/local-vs-centralized-condition.md|Local-first, centralize on second owner]] records why this is lazy instead of "always centralize from the start".

# Adr
- [[./adr/local-vs-centralized-condition.md|Local-first, centralize on second owner]]
  - Selected variant: centralize only once a second owner is real

# Requirements
SOLUTION:
- [[../solution-value-object.skill/solution-value-object.skill.md|solution-value-object]] - candidate first owner of a condition
- [[../solution-entity-invariant.skill/solution-entity-invariant.skill.md|solution-entity-invariant]] - candidate first owner of a condition
- [[../solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] - the mechanism applied once a second owner is found

# Template Skill Mutations
PROJECT:
- [[./Implementation/decision-table.md|decision-table.md]] - decision table - shows the code each branch produces, since this solution creates nothing itself

# Rule

## MUST
- Count real owners before deciding — never centralize because a condition merely *looks like* it will be needed elsewhere.
  - Risk: centralizing on a guess produces a shared abstraction with one caller, which is pure indirection until (if ever) the guess comes true.
  - Fix: apply `solution-domain-rule` only once a second file in the codebase actually needs the same condition, as in [[./Implementation/decision-table.md|decision-table.md]]'s "two owners" branch.
- Re-run this decision, not skip it, the next time a third owner of the same condition appears.
  - Risk: assuming "already centralized, nothing to decide" skips checking whether the third owner's version of the condition actually matches `EmailRule`'s.
  - Fix: verify the third owner can call the existing `EmailRule` unchanged before treating the decision as already resolved.

# Check list
- [ ] `solution-domain-rule` is applied if and only if two or more owners of the same condition exist right now.
- [ ] No owner keeps a local copy of a condition that has already been centralized.
