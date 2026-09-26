---
name: plateau-core--csproj-shared-tests
description: Project Shared.Tests in the plateau-core plateau — the dedicated test project for Shared, referencing Shared only
whenToUse: when adding a Gherkin scenario or unit test for a Shared primitive/marker, or checking that Shared.Tests keeps to Shared's own zero-project-reference boundary
domain: skill
type: template
plateau: core
version: 20260924000000
tags:
  - skill/template/csproj
  - plateau/core
created_by:
  - "[[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]]"
---

# Goal
- Give `Shared` a dedicated test project referencing `Shared` and nothing else — mirroring `Shared`'s own zero project references.
- Prove `Shared`'s primitives/markers with value-shaped Gherkin scenarios alongside plain unit tests, in one project.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/Shared.Tests.csproj.create|Shared.Tests.csproj]]

# Core Principles
- Scenarios are value-shaped: given one or more primitive values, prove how they compare/combine — never "is this input valid" (a module concern).
- Unit tests and Gherkin scenarios live together in this one project — never a second project for the scenarios.
- Runs on the VSTest runner (xUnit v2 + Reqnroll.xUnit — not xunit.v3, see [[skills/dotnet/test/solution-conformance-testing-in-dotnet.skill/adr/xunit-v2-until-stryker-supports-xunit-v3|ADR]]); `reqnroll.json` points the html formatter at `reqnroll_report.html` and the message formatter at `reqnroll_messages.ndjson`.

# Structure

## Solution place
```
/tests/Shared.Tests
```

## Project Structure
- /Shared.Tests
  - /Rules/{Rule}.feature
  - /StepDefinitions/[{Rule}Steps.cs](skills/dotnet/architecture/plateau/plateau-core/structure/Shared.Tests/classes/plateau-core--class-shared-rule-steps.skill.md)
  - reqnroll.json
  - Shared.Tests.csproj

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Rules/{Rule}.feature | Gherkin scenarios for one Shared primitive | |
| /StepDefinitions/{Rule}Steps.cs | Bindings asserting against the real `Shared` type | [[skills/dotnet/architecture/plateau/plateau-core/structure/Shared.Tests/classes/plateau-core--class-shared-rule-steps.skill\|class-shared-rule-steps]] |

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| Microsoft.NET.Test.Sdk | central | test host |
| xunit | central | assertions, xUnit v2 on VSTest (see [[skills/dotnet/test/solution-conformance-testing-in-dotnet.skill/adr/xunit-v2-until-stryker-supports-xunit-v3|ADR]]) |
| xunit.runner.visualstudio | central | IDE/CLI discovery |
| Reqnroll.xUnit | central | Gherkin binding + html/message reports |
| coverlet.collector | central | coverage |

## What Does NOT Belong Here
- Any module-specific concept — a module's scenarios go in that module's own `.Tests` project.
- A reference to any project other than `Shared`.

## Allowed Dependencies
- `Shared` — nothing else.

# Rules
MUST:
- Reference `Shared` and nothing else — a wider reference would let a test pass by relying on something `Shared` may not depend on.
- Keep unit tests and Gherkin scenarios in this one project.
- Set `<TreatWarningsAsErrors>false</TreatWarningsAsErrors>` for the Reqnroll-generated code-behind.
- Never introduce a module-specific concept into a `Shared.Tests` scenario.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/Shared.Tests.csproj.create|Shared.Tests.csproj]]

# Check list
- [ ] `Shared.Tests.csproj` references only `Shared` plus the five test packages (versionless).
- [ ] `/Rules` + `/StepDefinitions` + `reqnroll.json` present.
- [ ] Every `Given/When/Then` has a matching, non-duplicated step method asserting the real `Shared` type.
