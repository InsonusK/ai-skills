---
name: csproj-module-domain-rules-tests
description: Project {Module}.Domain.Rules.Tests in the v1 plateau
whenToUse: when proving a rule's own IsValid()/Check() correctness, isolated from the broader Entity/VO mutation surface of {Module}.Domain.Tests
domain: skill
type: template
plateau: v1
version: 20260824150000
tags:
  - skill/template/csproj
  - plateau/v1
created_by:
  - "[[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
---

# Goal
- Give `{Module}.Domain.Rules` its own dedicated test project, mirroring the one-test-project-per-production-project pattern established for the base five projects
- Isolate `{Module}.Domain.Rules`'s mutation-testing surface from `{Module}.Domain.Tests`'s broader one

__Applied solutions:__
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.create.md|{Module}.Domain.Rules.Tests.csproj.create]]

# Core Principles
- References `{Module}.Domain.Rules` only
- Takes `.feature` files from two sources: its own `/Rules` folder (rule-only edge cases) and, linked in, every file under `{Module}.Domain.Rules.Spec` (scenarios shared with `{Module}.Domain.Tests`/`{Module}.Application.Tests`)
- Step definitions here call the rule's own `Check()`/`IsValid()` directly — never a VO, Entity, or validator adapter

__Applied solutions:__
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.create.md|{Module}.Domain.Rules.Tests.csproj.create]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Domain.Rules.Tests
```

## Project Structure
- /{Module}.Domain.Rules.Tests
  - /Rules
    - {Rule}.feature   (rule-only scenarios, not shared with other layers)
  - /StepDefinitions
    - [{Rule}RuleSteps.cs](./classes/plateau-v1--class-rule-rule-steps.skill.md)
  - {Module}.Domain.Rules.Tests.csproj

`{Module}.Domain.Rules.Tests.csproj` links `{Module}.Domain.Rules.Spec` in:
```xml
<ItemGroup>
  <None Include="..\{ModuleName}.Domain.Rules.Spec\**\*.feature" Link="Rules\Shared\%(RecursiveDir)%(Filename)%(Extension)" />
</ItemGroup>
<ItemGroup>
  <ProjectReference Include="..\{ModuleName}.Domain.Rules\{ModuleName}.Domain.Rules.csproj" />
</ItemGroup>
```

__Applied solutions:__
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.create.md|{Module}.Domain.Rules.Tests.csproj.create]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Rules | Rule-only Gherkin scenarios, plus every file linked in from `{Module}.Domain.Rules.Spec` | |
| /StepDefinitions | Bindings that call `{Module}.Domain.Rules`'s real `Check()`/`IsValid()` | [[./classes/plateau-v1--class-rule-rule-steps.skill.md\|class-rule-rule-steps]] |

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `Reqnroll.xUnit` | latest stable | Run `.feature` files as xUnit tests |
| `coverlet.collector` | latest stable | Collect coverage during `dotnet test` |
| `Microsoft.NET.Test.Sdk` | latest stable | Test SDK required by xUnit/Reqnroll |

## Allowed Dependencies
- `{Module}.Domain.Rules` — nothing else

__Applied solutions:__
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.create.md|{Module}.Domain.Rules.Tests.csproj.create]]

# Rules
MUST:
- Reference `{Module}.Domain.Rules` and nothing else
- Link the entire `{Module}.Domain.Rules.Spec` directory in via `<None Include>`, never copy its scenario text
- Step definitions call `{Rule}.Check()`/`.IsValid()` directly
MUST NOT:
- Reference `{Module}.Domain`, `{Module}.Application`, or any other module project
- Duplicate a scenario already present in `{Module}.Domain.Rules.Spec` inside this project's own `/Rules` folder

__Applied solutions:__
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.create.md|{Module}.Domain.Rules.Tests.csproj.create]]

# Check list
- [ ] References `{Module}.Domain.Rules` only
- [ ] `{Module}.Domain.Rules.Spec/**/*.feature` linked in via `<None Include>`
- [ ] Every scenario in the linked spec has a passing step-definition binding here, regardless of classification tag

__Applied solutions:__
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.create.md|{Module}.Domain.Rules.Tests.csproj.create]]
