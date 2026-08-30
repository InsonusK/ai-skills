---
description: The registry-driven, recursive call-graph check proving every Entity write of a multi-field-rule-guarded property also calls that rule
project_name: "{Module}.Domain.Tests"
name: "GuardedPropertyRuleCoverageTests.cs"
element_kind: class
change_kind: create
tags:
  - solution/cecil-architecture-tests
  - element/guarded-property-rule-coverage-tests-cs
---

# Goals
- Prove that every public/internal Entity method/setter/constructor that writes a rule-guarded property also calls that rule — the one fact a `.feature` scenario proving the rule's own correctness cannot prove by construction

# Core Principles
- Kept in its own class, separate from the three single-pass checks in `{Module}ArchitectureTests.cs` — the recursive call-graph walk is materially more complex and a broken registry entry shouldn't bury a broken simple check's own failure
- The coverage registry (`Dictionary<(Entity, Property), Rule[]>`) lives in this test class, never in `Domain`/`Domain.Rules` production code
- A new guarded rule adds one registry line — never a new bespoke test class

# Implementation changes

Full worked implementation (`TaskModule`'s `TaskLink` entity), the recursive `CollectWritesAndCalls` walk with its `visited` guard, and the detailed rationale for why every public/internal entry point is checked (including the raw setter itself), why this doesn't scan beyond `Domain.dll`, and why the check can't live inside the property setter: [[skills/dotnet/architecture/v3/solutions/solution-cecil-architecture-tests.skill/examples/guarded-property-coverage.md|guarded-property-coverage.md]].

# Rule changes

## MUST
- Keep the coverage registry inside this test class
- Guard the recursive walk with a `visited` set keyed by `MethodDefinition`
- Add one registry line per newly-guarded property, never a new test class

## MUST NOT
- Scan any assembly beyond `{Module}.Domain`
- Put rule-check logic inside an individual property setter for a multi-property rule

# Check list
- [ ] Every guarded `(Entity, Property)` pair has a registry entry
- [ ] The recursive walk carries a `visited` guard
- [ ] Guarded setters are narrowed to `private` wherever the write pattern allows it
