---
description: Step definitions binding a Gherkin feature file to a Shared primitive's behavior
project_name: "Shared.Tests"
name: "{Rule}Steps"
element_kind: class
change_kind: create
---

# Goals
- Prove every scenario in `Rules/{Rule}.feature` against `Shared`'s real primitive/result-helper behavior.

# Core Principles
- `Shared` holds cross-cutting primitives, not business or orchestration logic, so its scenarios are value-shaped: given one or more primitive values, prove how they compare or combine — never "is this input valid" (that belongs to a module's own Domain/Application).

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Step definitions for one primitive's scenarios | {Rule}Steps | ConflictResultSteps | {Rule}Steps.cs | ConflictResultSteps.cs |

# Implementation changes
```csharp
[Binding]
public sealed class {Rule}Steps
{
    private {Primitive} _first = null!;
    private {Primitive} _second = null!;
    private bool _areEqual;

    [Given(@"two {Rule} values ""(.*)"" and ""(.*)""")]
    public void GivenTwoValues(string a, string b)
    {
        _first = new {Primitive}(a);
        _second = new {Primitive}(b);
    }

    [When(@"they are compared")]
    public void WhenCompared() => _areEqual = _first.Equals(_second);

    [Then(@"they are considered equal")]
    public void ThenEqual() => Assert.True(_areEqual);

    [Then(@"they are considered different")]
    public void ThenDifferent() => Assert.False(_areEqual);
}
```

# Rule changes

## MUST
- Assert against the real `Shared` type — never a hand-written stand-in.
  - Risk: the scenario can stay green after the real primitive's comparison/combination logic breaks.
  - Fix: construct and assert on the real declared type from `Shared`.
- Never introduce a module-specific concept into a `Shared.Tests` scenario.
  - Risk: a module-specific scenario here would only be discoverable by someone browsing `Shared`, not the module it actually concerns.
  - Fix: keep `Shared.Tests` scenarios scoped to genuinely cross-cutting primitives.

# Check list
- [ ] Every `Given/When/Then` in `{Rule}.feature` has a matching, non-duplicated step method.
- [ ] `{Rule}Steps` asserts against the real `Shared` type.

# Unittest TestCases
- [ ] WHEN two equal values are compared THEN `ThenEqual` passes.
- [ ] WHEN two different values are compared THEN `ThenDifferent` passes.
