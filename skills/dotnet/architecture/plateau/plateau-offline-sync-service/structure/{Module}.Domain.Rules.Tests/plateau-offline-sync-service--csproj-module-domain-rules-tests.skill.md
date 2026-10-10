---
name: plateau-offline-sync-service--csproj-module-domain-rules-tests
description: Project {Module}.Domain.Rules.Tests in the plateau-offline-sync-service plateau — tests its production counterpart within the mirrored Allowed Dependencies
whenToUse: when adding a scenario proving a Rule's own Check() / IsValid() / IRuleBuilder extension, or checking the Domain.Rules test isolation
domain: skill
type: template
plateau: offline-sync-service
version: 20261009220001
tags:
  - skill/template/csproj
  - plateau/offline-sync-service
created_by:
  - "[[skills/dotnet/architecture/solutions/solution-domain-shared-rules.skill/solution-domain-shared-rules.skill|solution-domain-shared-rules]]"
  - "[[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]]"
  - "[[skills/dotnet/architecture/solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill|solution-cecil-architecture-tests]]"
---

# Goal
- Give `{Module}.Domain.Rules` its catalog-selected test project with its production dependency boundary mirrored.
- Prove every scenario in the rule's `.feature` file directly against `IsValid()` / `Check()` / the `IRuleBuilder` extension — the one place the rule's own correctness is proven in isolation.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-domain-shared-rules.skill/solution-domain-shared-rules.skill|solution-domain-shared-rules]] - [[skills/dotnet/architecture/solutions/solution-domain-shared-rules.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.create|{Module}.Domain.Rules.Tests.csproj]]

# Core Principles
- Apply the linked [testing conventions](#testing-conventions).
- Apply the mirrored [dependency boundary](#allowed-dependencies), including any explicitly applied architecture-test extension.
- Links in both its own `features/*.feature` (rule-only edge cases) and, physically, `{Module}.Domain.Rules.Spec`'s shared `.feature` files, generating its own Reqnroll fixture bound to its own step definitions.
- Proves every scenario regardless of `@format`/`@semantic`/`@domain` tag — the other layers only re-prove their applicable subset.

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects), [binding mechanics](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md), [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for layout, bindings, assertions, packages and runner configuration.

# Structure

## Solution place
```
/tests/{Module}.Domain.Rules.Tests
```

## Project Structure
With VP4, local features cover rule-only edge cases and `/Architecture` hosts dead-rule and rejection-code uniqueness checks. The shared spec contributes every classification folder.

Apply the linked [testing conventions](#testing-conventions).

The whole spec directory is linked (this project proves every scenario regardless of tag). `ReqnrollFeatureFiles`, never `None` — Reqnroll generates no test for a `None` item (see [[skills/dotnet/architecture/solutions/solution-domain-shared-rules.skill/adr/spec-folders-per-classification|ADR]]):
```xml
<ReqnrollFeatureFiles Include="..\..\src\Modules\{ModuleName}\{ModuleName}.Domain.Rules.Spec\**\*.feature" Link="features\Shared\%(RecursiveDir)%(Filename)%(Extension)" />
```

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| {Rule}RuleSteps.cs | Bindings proving the rule's own `IsValid()`/`Check()` | [[skills/dotnet/architecture/plateau/plateau-offline-sync-service/structure/{Module}.Domain.Rules.Tests/classes/plateau-offline-sync-service--class-rule-steps.skill\|class-rule-steps]] |
| /Architecture/{Module}RuleArchitectureTests.cs | Cecil: dead-rule detection + rejection-code uniqueness/format | [[skills/dotnet/architecture/plateau/plateau-offline-sync-service/structure/{Module}.Domain.Tests/classes/plateau-offline-sync-service--class-architecture-tests.skill\|class-architecture-tests]] |

## What Does NOT Belong Here
- The VO/entity fail-fast adapter proof — that is `{Module}.Domain.Tests`.
- The DTO-validator collect-all adapter proof — that is `{Module}.Application.Tests`.
- A project reference outside the production Allowed Dependencies and any explicitly applied architecture-test extension.

## Allowed Dependencies
- Reference `{Module}.Domain.Rules` and mirror its production project's assembled [Allowed Dependencies](skills/dotnet/architecture/plateau/plateau-offline-sync-service/structure/{Module}.Domain.Rules/plateau-offline-sync-service--csproj-module-domain-rules.skill.md#allowed-dependencies); no wider project boundary.
- With the Cecil companion, also reference `Mono.Cecil` and the production assemblies inspected by [the dead-rule scan](skills/dotnet/architecture/solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.extend.md).

# Rules
MUST:
- Apply the linked [testing conventions](#testing-conventions).
- Apply the mirrored [dependency boundary](#allowed-dependencies), including any explicitly applied architecture-test extension.
- Host **only** the two rules-only Cecil checks (`{Module}RuleArchitectureTests`) in `/Architecture` — exception-scoping and guarded-property-coverage belong in `{Module}.Domain.Tests`.
- Prove every scenario in the rule's `.feature` directly against `IsValid()`/`Check()`.
- Never duplicate scenario text — link the physical `.feature` file from `{Module}.Domain.Rules.Spec`.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-domain-shared-rules.skill/solution-domain-shared-rules.skill|solution-domain-shared-rules]] - [[skills/dotnet/architecture/solutions/solution-domain-shared-rules.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.create/{Rule}RuleSteps.cs.create|{Rule}RuleSteps.cs]]

# Check list
- [ ] References match the mirrored production boundary and any explicitly applied architecture-test extension.
- [ ] Proves every `.feature` scenario against `IsValid()`/`Check()`.
- [ ] No scenario text duplicated across the three rule-proving test projects.

Feature files follow [[skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md|cucumber-testing]] for mandatory feature type and scenario/Examples category tags; architecture classification tags and the existing documentary exceptions remain separate.
