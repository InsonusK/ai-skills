---
name: consumer-instructions-without-catalog-status
description: Separate published skill instructions from the catalog's development tracking
problem: Consumer skills contain migration-wave and draft-authoring notes meaningful only to catalog maintainers.
decision: Keep catalog progress in authoring tracking and express only the application contract, capability limits and required project choices in consumer instructions.
status: accepted
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
A skill used in another service must tell its agent what to implement. A catalog
migration wave, unpublished draft timeline or lack of an earlier catalog consumer
neither defines application behavior nor helps that agent implement it. Removing
those notes must not erase actual support limits or invent finalized policies.

# Selected variant
[Application contracts with separate catalog tracking](#application-contracts-with-separate-catalog-tracking)

Owner direction, 2026-11-28.

# Searched variants

## Application contracts with separate catalog tracking

**Selected.**

### Description
Keep runtime instructions and templates focused on application responsibilities,
guarantees, supported mechanisms and project-supplied choices. Maintain catalog
implementation status, migration sequencing and release readiness in authoring
plans and audit records. Descriptions of stateful artifacts in authoring/testing
skills remain legitimate when those states are part of the artifact's contract.

### Benefits
- External consumers receive stable instructions without catalog-work context.
- Real capability limits and deployment choices remain visible.
- Incomplete catalog work stays discoverable in its owning plan.

### Costs
- Authors must maintain project tracking separately from consumer instructions.
- Removing a development note can reveal underlying rule contradictions that
  require correction before publishing the skill.

## Inline catalog status in every affected skill

### Description
Annotate consumer instructions with draft labels, pending work and migration waves.

### Benefits
- Maintainers see unfinished work in place.

### Costs
- External agents load unrelated or contradictory catalog-development instructions.
