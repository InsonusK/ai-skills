---
name: plateaus are coded by their VP combination, not named by topic
description: How a plateau is identified, so equivalent plateaus are recognisable across stacks and the plateau set does not grow by naming
problem: How should a plateau be identified so that the same VP combination is recognisable across stacks and a reader can tell what a plateau contains?
decision: Every plateau carries a code `{stack}{kind}{common}.{specific}`; `{common}` numbers its combination of common-VP Variants in a shared registry (same combination, same number in every stack), `{specific}` numbers its set of stack VPs in the catalog (000 = none); the descriptive name becomes a Title column; a code changes with its combination.
tags:
  - concern/architecture
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Plateaus were named by what they are about (`plateau-persistent-service`, `plateau-domain-service`). The names say nothing about which VP combination a plateau realizes, the same combination gets unrelated names in different stacks, and a new name is invented for every new combination. With common Variation Points now shared across stacks, a plateau's identity should state its combination and match across stacks.

# Selected variant
[[#Two-part code by combination]]

# Searched variants

## Two-part code by combination

**Selected.**

### Description
Code `{stack}{kind}{common}.{specific}` — one registered letter per stack and per kind; `{common}` from the shared common-plateau registry (one number per combination of common-VP Variants); `{specific}` from the catalog's own table of stack-VP combinations, `000` meaning none. The old name stays as the matrix's Title. When a combination changes (a VP becomes common, a solution is added), the code changes in the same change.

### Benefits
- The same common combination has the same number in every stack, so `GW003` and `DW003` are recognisably the same service shape.
- The same `{specific}` number within a catalog means the same stack-VP set, so plateaus differing only in their common part are easy to line up.
- A plateau's content can be read off its code through two small tables; no name has to be invented per combination.

### Costs
- A bare code is unreadable without the matrix; the Title column is needed as its decoding.
- Codes change when a VP is promoted to common or a combination changes, so plateaus must be renamed — mechanical, but it touches every file carrying the plateau's name.
- Needs a shared registry maintained across stacks.

## Keep descriptive names

### Description
Keep naming plateaus by topic and record the VP combination only in the matrix.

### Benefits
- Names are readable in conversation and in file listings.
- No renames when combinations change.

### Costs
- The same combination is named differently per stack; nothing makes equivalent plateaus recognisable.
- Every new combination needs a new invented name, which encourages new plateaus per topic rather than per real combination.

## Single sequential number per catalog

### Description
Number plateaus `001, 002, ...` per catalog, with stack and kind letters, without encoding the combination.

### Benefits
- Stable: a number never changes.
- Simple to assign.

### Costs
- The number carries no meaning across stacks — `GW003` and `DW003` would be unrelated plateaus.
- The combination must still be read from the matrix; the code adds identity but no information.
