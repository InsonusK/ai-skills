---
name: archive-superseded-decisions
description: Preserve superseded decisions without registering them as current skill rules
problem: A replaced ADR can still present its original selection as active guidance when linked beside its replacement.
decision: Archive fully superseded ADRs under the owning skill's adr/superseded directory, remove active registration and retain a historical link from the replacement only.
status: accepted
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
An ADR preserves a decision at a point in time, while a skill gives current
instructions. Keeping contradictory records in the same active registration
makes agents and reviewers repeatedly reconcile history with the current rule.
Deleting the predecessor loses the reason the replacement was needed.

# Selected variant
[Archive history behind the replacement](#archive-history-behind-the-replacement)

Owner agreement, 2026-11-28.

# Searched variants

## Archive history behind the replacement

**Selected.**

### Description
Move a fully superseded ADR into the same skill's adr/superseded directory. Keep
its original decision, alternatives and selection, adding status and a relative
superseded_by path plus a visible historical notice. Only the replacement links
to the archived record as history. Active skill rules and registration reference
the replacement. Move exclusively owned assets and repair relative paths; leave
assets shared by active records accessible to their consumers. Partially amended
records remain active for their unaffected decisions, with amendment scope stated.

### Benefits
- The active contract points to the current choice.
- History retains the old rationale and the reason for replacement.
- Archival ownership stays with the skill that made the decision.

### Costs
- Moves require updating links, supporting assets and registration.
- Loaders may still copy archives or follow the historical link; moving a file
  alone does not guarantee that its contents are excluded from agent context.

## Keep superseded ADRs in active registration

### Description
Keep old and new decisions beside each other with a superseded notice.

### Benefits
- No file move or link repair.

### Costs
- Contradictory metadata and selected variants remain in the active reading path.

## Delete old ADRs and rely on Git

### Description
Remove obsolete records and retrieve their original reasoning from version history.

### Benefits
- The working tree contains only current decisions.

### Costs
- Discovering why a decision changed requires reconstructing Git history.
- Replacement rationale is harder to review from the current skill alone.
