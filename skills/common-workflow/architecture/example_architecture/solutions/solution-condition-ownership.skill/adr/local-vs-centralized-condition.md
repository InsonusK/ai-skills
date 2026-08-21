---
name: local-vs-centralized-condition
description: Whether a validation condition should be centralized as soon as a solution is designed, or only once a second owner genuinely needs it
problem: The "valid email format" condition is needed by both Email's constructor and ChangeCustomerEmailCommandValidator. Decide when EmailRule should be introduced.
decision: Keep the condition local to its first owner until a real second owner exists; only then apply solution-domain-rule to centralize it.
tags:
  - solution/condition-ownership
  - stack/dotnet
  - concern/documentation
  - concern/documentation/adr
---

# Problem

A validation condition can be written local to the first type that needs it, or centralized into a shared, reusable shape from the start. Both are legitimate; the question is when to choose which, for this module's "valid email format" condition specifically.

# Selected variant

**Selected variant:** [[#Centralize lazily, on the second owner (selected)]]

# Searched variants

## Centralize lazily, on the second owner (selected)

### Description

`Email`'s constructor gets its own local `IsValidFormat` predicate from the start. Only once `ChangeCustomerEmailCommandValidator` needs the same check does `solution-domain-rule` get applied, centralizing the condition into `EmailRule` and redirecting both owners.

### Benefits

- No shared abstraction exists until it has a genuine second caller — `EmailRule` is never "indirection with one user".
- Each owner stays fully self-contained and testable on its own for as long as it is the only owner, matching how `solution-value-object` and `solution-entity-invariant` are documented (each "works completely on its own").
- The decision of *when* to centralize becomes a checkable fact ("are there 2+ owners now?") instead of a judgment call repeated differently each time.

### Costs

- Requires an explicit redirect step (`solution-domain-rule`'s `.extend.md` files) when the second owner does appear, instead of the condition already living in its final place.
- Between the first and second owner, the condition genuinely is duplicated in spirit (even though only one copy of the code exists) — a change to the business rule at that point only needs one edit, but a developer must still know a second owner is coming.

## Centralize from the start

### Description

Always create `EmailRule` (or an equivalent shared shape) as soon as the module is designed, before any concrete second owner exists, and have `Email`'s constructor call it from day one.

### Benefits

- No redirect step is ever needed later — the final shape exists immediately.
- Removes any window where the condition could accidentally diverge between a "local" and a "centralized" version.

### Costs

- Produces a shared abstraction with exactly one caller for modules that never grow a second owner — pure indirection with no de-duplication benefit.
- Forces every module's author to predict, upfront, which conditions will eventually be needed in two places — a prediction this catalog's real solutions (`solution-value-object`, `solution-dto-property-validators`, `solution-domain-behaviour`) explicitly avoid making.

## Always keep every condition local, never centralize

### Description

`Email` and `ChangeCustomerEmailCommandValidator` each keep their own copy of the condition permanently; `solution-domain-rule` is never applied.

### Benefits

- Simplest possible rule: no decision to make, no redirect step, ever.
- Zero coupling between the two owners.

### Costs

- Once a real second (or third) owner exists, the same business rule now has to be edited in multiple files every time it changes, and nothing prevents the copies from silently drifting apart.
- Loses the specific benefit `solution-domain-rule` exists for: a rule proven for one owner being reused, unmodified, by another.
