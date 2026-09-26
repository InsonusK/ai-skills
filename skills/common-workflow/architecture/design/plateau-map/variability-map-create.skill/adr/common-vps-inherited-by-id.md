---
name: common VPs inherited by ID, not copied
description: How Variation Points shared by every backend web-service catalog reach each stack's Variability Map
problem: How should a Variation Point that every web-service stack shares be defined once and kept consistent across the stack catalogs' Variability Maps?
decision: One common map defines each shared VP under a permanent `VP-C###` ID; every bound stack map carries every agreed common VP by that ID with a detail status and a State (Inherited / Refined / Fixed) and restates nothing the common map owns; VPs are admitted one at a time — candidate, agreed concept, then detailed on each stack.
tags:
  - concern/architecture
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Backend web-service catalogs on different stacks (Go, dotnet, ...) answer largely the same variability questions — which storage, which inbound and outbound protocols, deferred tasks, outbox. A stack-agnostic starting list existed as a template each catalog copied and adapted. The copies drifted: one stack kept 7 VPs, another 14; the same outbound-call question was one VP in one stack and split by transport in the other; `DomainLogic` and `HttpApi` were baseline in one family and VPs in the other. Nothing tied a stack row back to the shared question, so nothing could detect the drift. How should a shared VP be defined once and stay consistent across stacks?

# Selected variant
[[#Inherited common map (selected)]]

# Searched variants

## Inherited common map (selected)

### Description
One common map (`templates/web-service-common-variability-map/`) owns each shared VP: question, Variants, Constraint, Realization depends on, and a concept section, under a permanent `VP-C###` ID. Every bound stack map carries every common VP in a `## Common Variation Points` table holding only ID, name, State (`Inherited` / `Refined` / `Fixed: {Variant}`), the stack's narrowing, `Realized by`, and `Migration`. A stack narrows, never widens. A VP moves 💡 candidate → 📐 concept (owner-agreed; every bound stack gets a ⏳ row) → ✅ detailed per stack (State, chosen realization per Variant, narrowing); a stack-local VP it covers is re-IDed to the common ID when that stack details it.

### Benefits
- Every shared fact lives in one place; a stack cannot silently re-cut a shared question.
- An absent row is a mechanical failure, so "forgot it" and "not relevant" can no longer be confused — "not relevant" is an explicit `Fixed: No`.
- Detailing each stack's realization right after the concept is agreed keeps the stack realizations to one concept, and the status icons show the owner which VPs are ideas, agreed, or thought through per stack.
- The `VP-C` prefix tells a reader of a stack map at a glance which VPs come from the shared definition.

### Costs
- Every bound stack map must be touched whenever a common VP is admitted or changed.
- Re-IDing a covered stack-local VP rewrites every reference to it in that stack's tree.
- Admission is slower than copying a list, because each VP needs discussion and per-stack design before it enters.

## Copy-what-applies template

### Description
Keep a stack-agnostic candidate list that each catalog copies, renames, and trims to its own baseline, re-verifying each Constraint locally.

### Benefits
- Each catalog is fully free to shape its own VPs.
- No cross-stack coordination on change.

### Costs
- The copies drift with no way to detect it — the situation this decision replaces.
- The shared list is never verified against a real stack, so its mistakes (a feature placed under the wrong parent, a VP that fails the "two teams, two answers" test) are copied rather than caught.

## Common map with a mapping column only

### Description
Stacks keep their own VP rows and IDs, and add a column naming which common VP (if any) each row corresponds to.

### Benefits
- No re-IDing; existing references stay valid.

### Costs
- The stack row still restates question, Variants, and Constraint, so the copies can diverge from the common definition while the mapping column claims they match.
- A common VP a stack never mapped is invisible — absence stays undetectable.
