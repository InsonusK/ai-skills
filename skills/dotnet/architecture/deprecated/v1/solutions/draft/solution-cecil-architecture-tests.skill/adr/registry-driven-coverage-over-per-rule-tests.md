---
name: registry-driven-coverage-over-per-rule-tests
description: How to guarantee that every Entity method writing a property guarded by a multi-field rule actually calls that rule — for the current rule and every future one.
problem: A rule author can add a Semantic/Domain rule and wire it into one Entity method while forgetting another (present or future) — no .feature scenario or unit test catches a call site that was never written.
decision: One generic, registry-driven Cecil test — a rule author adds one (Entity, Property) -> Rule line, not a new test.
---

# Problem

A rule spanning more than one Entity property (e.g. `TaskLinkSelfLinkRule` over `TaskLink.ParentId`+`ChildId`) can be correctly wired into one mutation point (`TaskLink.Create`) and silently skipped by another (a raw `internal` setter, or a different Handler building the Entity via object-initializer) — found for real during this solution's own development (`UpdateTaskLinkForTaskHandler` built `TaskLink` directly, bypassing `Create()` and its self-link check entirely). No `.feature` scenario proves this, because a `.feature` only proves the rule's own condition is correct on the adapter it was written against — it says nothing about paths nobody wrote a scenario for.

# Selected variant

**Selected variant:** [[#Registry + generic recursive Cecil walk, paired with narrowing setters to private]]

# Searched variants

## Registry + generic recursive Cecil walk, paired with narrowing setters to private (selected)

### Description

A `Dictionary<(Entity, Property), Rule[]>` registry, read by one generic `[Fact]` that walks every public/internal member of every Entity (methods, setters, init, constructors), recursively following calls into the Entity's own private/internal helpers, collecting which properties get written and which rules get called, and flagging any guarded property written without its required rule anywhere in the walk. See [[../examples/guarded-property-coverage.md|guarded-property-coverage.md]] for the full implementation. Paired with narrowing the guarded property's own setter to `private` wherever the write pattern allows it, closing the same gap at compile time for callers outside the Entity.

### Benefits

- Adding rule coverage for a new field group is one dictionary entry, not a new test class — the "did I remember" burden shrinks to something reviewable in a one-line diff, not something that requires re-deriving the whole call graph by eye.
- Works forever, automatically, for code written after the registry entry was added — a future method on the same Entity that forgets to call the rule fails the build the moment it exists, with no one needing to remember the rule exists.
- Catches the setter itself as a violation, not just callers of it — closing the exact class of bug (`internal` setter as an escape hatch) this ADR exists to prevent, independent of who currently calls it.

### Costs

- The recursive call-graph walk (`CollectWritesAndCalls`) is meaningfully more code than the other three architecture tests in this solution, and needs its own test class (see [[../examples/guarded-property-coverage.md]]) to stay reviewable.
- The registry itself can be forgotten exactly like the original wiring can — mitigated by process (PR review on `Domain`/`Domain.Rules` requiring a human check "did this add a multi-field rule without a registry line"), not eliminated by tooling alone. A further, not-yet-built backstop would be a cheaper test asserting every multi-field rule's `Check()` appears in at least one registry entry.

## Bespoke Cecil test per coupled-field group

### Description

Hand-write a dedicated `[Fact]` per rule (e.g. `TaskLinkSelfLinkRule_IsCalledByEveryTaskLinkMutation`) checking only that one rule's wiring.

### Benefits

- No shared registry/recursion machinery to design up front.

### Costs

- N rules become N near-identical test methods; the discipline required to remember to write test N+1 when rule N+1 is added is exactly the same discipline the whole mechanism exists to replace — the "agent forgets" problem reappears one level up, at "did they write the test" instead of "did they wire the rule."

## Exhaustive multi-assembly external-caller scan

### Description

Keep guarded setters `internal`, and instead load every assembly reachable via `InternalsVisibleTo` (plus `Domain.dll` itself), scanning all of them for calls to the setter.

### Benefits

- Does not require narrowing any property's visibility.

### Costs

- Answers a fundamentally harder question than needed: Cecil exposes "what does a method call," not "who calls a method" — finding every caller means loading and walking every assembly that could contain one, a list that must be kept in sync by hand as new `InternalsVisibleTo` targets are added.
- Gives no guarantee at all for a `public` member — there is no bounded set of "every assembly that might ever reference it," especially for a `Domain.Rules`-shaped project meant to be consumed by other services. `private` sidesteps the question entirely instead of trying to answer an open-ended version of it.

## Rule check embedded in the property setter itself ("smart setter")

### Description

Give the guarded property's own setter the multi-field validation logic, reading sibling properties at assignment time.

### Benefits

- No separate method/constructor call needed to trigger the rule — assigning the property is enough.

### Costs

- Unsafe for any rule spanning more than one property: object-initializer/constructor property assignment happens in a fixed order, so a sibling property may still hold its default or previous value when this setter's check runs. The check validates a transient combination, not the final one — can silently accept an invalid final state or reject a valid one, depending on assignment order. Only safe when every coupled value is received atomically, in one call (a constructor or a dedicated method) — which a setter, by definition, cannot do for more than its own single value.
