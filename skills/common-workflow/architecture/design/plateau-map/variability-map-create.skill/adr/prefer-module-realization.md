---
name: prefer a module realization for a Variation Point
description: Which shape a Variation Point's realization should take, so adding a VP does not multiply plateaus or scatter one concern across them
problem: A plateau with a full code example per VP combination grows with every VP, and a concern every plateau shares (testing) ends up described in each plateau as well as in its own skills — which realization shape should a VP be cut for?
decision: Cut a VP so each Variant attaches as a module — its own project/package behind a port the baseline declares, wired at the composition root — and accept a structural realization (one that changes files the baseline or another VP's solution owns) only when no port can carry the VP.
tags:
  - concern/architecture
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Every VP combination a stack built became its own plateau with a full code example. Two effects showed up (owner, 2026-10-07):

1. **The plateau set grows with the combinations.** Eleven common VPs are admitted; the common-plateau registry already retired three numbers when one VP became common, and each new combination needs a new plateau and example.
2. **A shared concern is described in several places.** Testing is stated in the stack-agnostic test skills, in each stack's test skills, and again inside plateaus — the dotnet plateaus each carry their own `*.Tests` structure skills — so no single place says how a service is tested.

Both come from realizing a VP by changing the service's own structure. Which realization shape should a VP be cut for?

# Selected variant
[[#Module realization preferred]]

# Searched variants

## Module realization preferred

**Selected.**

### Description
A VP is cut so each Variant is a module: its own project/package (per the stack's unit of isolation) behind a port the baseline declares, registered at the composition root, touching no file the baseline or another VP's solution owns. Example: every external integration (store, broker, outbound call) lives in its own infrastructure project/package, so one stack-wide rule — "a test project/package sits beside every such module" — covers its tests without any plateau describing them. A structural realization stays legitimate when the VP changes what the service's own files contain (e.g. `DomainLogic`), and is recorded as such.

### Benefits
- Adding a module VP changes the composition root only; no plateau is forked for it — the same outcome [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md#fdn-never-forks-the-plateau-tree|delta-conflict-detection]] already requires for `FDN`.
- A rule stated per kind of module (how it is placed, how it is tested) applies to any program in the stack, not only to one built from a catalog's plateaus.
- Shared concerns leave the plateaus: the baseline names the skills it uses instead of restating them.

### Costs
- The baseline must declare the ports up front, so it is heavier than the simplest possible service.
- A combination is no longer proven by a plateau example; each module needs its own tests at its port, and constrained combinations still need a composed example.
- Plateau codes still number every combination of common-VP Variants ([[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/adr/plateau-code-by-combination|plateau-code-by-combination]]); whether module VPs stay in the code is undecided.

## Plateau per VP combination

### Description
The status quo: a VP is realized by whatever change its solution needs, and each built combination is a plateau with its own structure skills and full example.

### Benefits
- Every built combination is proven by a runnable example.
- A plateau is read alone, with no ports to understand first.

### Costs
- The two effects in the problem statement.
- A change to a shared concern must be repeated in every plateau that restates it.

## Every VP as an external library

### Description
Extend [[skills/common-workflow/architecture/design/solution-create.skill/adr/contract-vp-realized-by-libraries|contract-vp-realized-by-libraries]] to all VPs: a spec repository and a library repository per stack for each.

### Benefits
- Each VP is validated by its own conformance CI.

### Costs
- Several repositories per VP; justified only for a mechanism fixed by a stack-agnostic contract.
- A VP that shapes the service's own code (ports, inbound APIs, domain model) has nothing to ship as a library.
