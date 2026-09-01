---
description: Step definitions binding a Gherkin feature file to a DTO/command/query contract's shape
project_name: "{Module}.Interfaces.Tests"
name: "{Rule}Steps"
element_kind: class
change_kind: create
tags:
  - solution/dotnet-conformance-testing
  - element/rulesteps

---

# Goals
- Prove every scenario in `Rules/{Rule}.feature` against `{Module}.Interfaces`'s real declaration — that the contract's shape (equality, serialization round-trip) holds, since `{Module}.Interfaces` has no behavior to validate.

# Core Principles
- `{Module}.Interfaces` is declarations-only, so its scenarios are shape-shaped, not validation-shaped: given a value, prove it survives round-tripping (or compares correctly), not that it is "valid".

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Step definitions for one contract's scenarios | {Rule}Steps | ChangeCustomerEmailCommandSteps | {Rule}Steps.cs | ChangeCustomerEmailCommandSteps.cs |

# Implementation changes
```csharp
[Binding]
public sealed class {Rule}Steps
{
    private {Dto} _original = null!;
    private {Dto} _roundTripped = null!;

    [Given(@"a {Rule} value with ""(.*)""")]
    public void GivenAValue(string value) => _original = new {Dto}(value);

    [When(@"it is serialized and deserialized")]
    public void WhenRoundTripped() =>
        _roundTripped = JsonSerializer.Deserialize<{Dto}>(JsonSerializer.Serialize(_original))!;

    [Then(@"the result equals the original")]
    public void ThenEqualsOriginal() => Assert.Equal(_original, _roundTripped);
}
```

# Rule changes

## MUST
- Assert against the real `{Dto}`/command/query type — never a hand-written stand-in shape.
  - Violation: the step asserts against an anonymous object shaped like `{Dto}` instead of `{Dto}` itself.
  - Risk: the scenario can stay green after the real contract's shape changes in a breaking way.
  - Fix: construct and assert on the real declared type.
- Cover equality and serialization round-trip, not validity — validity belongs to `{Module}.Application.Tests`/`{Module}.Domain.Tests`.
  - Risk: duplicating a validation check here creates a second copy of a condition that can drift from the real validator's.
  - Fix: keep scenarios here to shape/equality/serialization only.

# Check list
- [ ] Every `Given/When/Then` in `{Rule}.feature` has a matching, non-duplicated step method.
- [ ] `{Rule}Steps` asserts against the real declared type, not a stand-in shape.

# Unittest TestCases
- [ ] WHEN a value is serialized and deserialized THEN the round-tripped value equals the original.
