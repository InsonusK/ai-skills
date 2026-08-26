---
name: plateau-v1--class-guarded-property-rule-coverage-tests
description: Class GuardedPropertyRuleCoverageTests in the v1 plateau — the registry-driven, recursive call-graph Cecil check
whenToUse: when proving that every public/internal Entity write of a multi-field-rule-guarded property also calls that rule
domain: skill
type: template
plateau: v1
version: 20260824150000
tags:
  - skill/template/class
  - plateau/v1
created_by:
  - "[[../../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]]"
---

# Goal
- Prove that every public/internal Entity method/setter/constructor that writes a rule-guarded property also calls that rule — the one fact a `.feature` scenario proving the rule's own correctness cannot prove by construction

__Applied solutions:__
- [[../../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] - [[../../../../../solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/GuardedPropertyRuleCoverageTests.cs.create.md|GuardedPropertyRuleCoverageTests.cs.create]]

# Core Principles
- Kept in its own class, separate from `{Module}ArchitectureTests` — the recursive call-graph walk is materially more complex, so a broken registry entry shouldn't bury a broken simple check's own failure
- The coverage registry (`Dictionary<(Entity, Property), Rule[]>`) lives in this test class, never in `Domain`/`Domain.Rules` production code
- Only scans `{Module}.Domain` — never `{Module}.Application` or any other assembly; narrowing a guarded setter to `private` is what actually closes an external bypass, this test only catches what's left reachable within `Domain` itself
- A new guarded rule adds one registry line — never a new bespoke test class

# Implementation
```csharp
//Skill: class-guarded-property-rule-coverage-tests
//Plateau: v1
//Version: 20260824150000

public sealed class GuardedPropertyRuleCoverageTests
{
    private static readonly Dictionary<(string Entity, string Property), string[]> RequiredRuleChecks = new()
    {
        // [(nameof(Entity), nameof(Entity.Property))] = [$"{nameof(SomeRule)}.{nameof(SomeRule.Check)}"],
    };

    [Fact]
    public void GuardedProperties_AreOnlyWrittenByMembersThatCallTheirRequiredRuleChecks()
    {
        // recursive CollectWritesAndCalls walk with a `visited` guard —
        // see solution-cecil-architecture-tests examples/guarded-property-coverage.md for the full implementation
    }
}
```

Full worked implementation (the recursive `CollectWritesAndCalls` walk, the `visited` guard, and the detailed rationale for why every public/internal entry point is checked — including the raw setter itself — and why this doesn't scan beyond `Domain.dll`): [[../../../../../solutions/solution-cecil-architecture-tests.skill/examples/guarded-property-coverage.md|guarded-property-coverage.md]].

__Applied solutions:__
- [[../../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] - [[../../../../../solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/GuardedPropertyRuleCoverageTests.cs.create.md|GuardedPropertyRuleCoverageTests.cs.create]]

# Rules
MUST:
- Keep the coverage registry inside this test class
- Guard the recursive walk with a `visited` set keyed by `MethodDefinition`
- Add one registry line per newly-guarded property, never a new test class
MUST NOT:
- Scan any assembly beyond `{Module}.Domain`
- Put rule-check logic inside an individual property setter for a multi-property rule

__Applied solutions:__
- [[../../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] - [[../../../../../solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/GuardedPropertyRuleCoverageTests.cs.create.md|GuardedPropertyRuleCoverageTests.cs.create]]

# Check list
- [ ] Every guarded `(Entity, Property)` pair has a registry entry
- [ ] The recursive walk carries a `visited` guard
- [ ] Guarded setters are narrowed to `private` wherever the write pattern allows it

__Applied solutions:__
- [[../../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] - [[../../../../../solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/GuardedPropertyRuleCoverageTests.cs.create.md|GuardedPropertyRuleCoverageTests.cs.create]]
