---
name: plateau-domain-service--csproj-module-domain-tests
description: Project {Module}.Domain.Tests in the plateau-domain-service plateau — tests its production counterpart within the mirrored Allowed Dependencies
whenToUse: when adding a Gherkin scenario or unit test for an entity invariant, a domain service, or a strict Value Object
domain: skill
type: template
plateau: domain-service
version: 20261009220001
tags:
  - skill/template/csproj
  - plateau/domain-service
created_by:
  - "[[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]]"
---

# Goal
- Give `{Module}.Domain` its catalog-selected test project with its production dependency boundary mirrored.
- Exists only once `{Module}.Domain` exists (VP1).

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create|{Module}.Domain.Tests.csproj]]

# Core Principles
- Apply the linked [testing conventions](#testing-conventions).
- Scenarios are validator-shaped: an input goes in, valid/invalid comes out — proven against the real entity method / VO constructor, asserting the `DomainException` code on failure.
- Apply the mirrored [dependency boundary](#allowed-dependencies), including any explicitly applied architecture-test extension.

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects), [binding mechanics](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md), [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for layout, bindings, assertions, packages and runner configuration.

# Structure

## Solution place
```
/tests/{Module}.Domain.Tests
```

## Project Structure
Apply the linked [testing conventions](#testing-conventions).

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| {Rule}Steps.cs | Bindings asserting entity/VO behavior against the real types | [[skills/dotnet/architecture/plateau/plateau-domain-service/structure/{Module}.Domain.Tests/classes/plateau-domain-service--class-module-domain-rule-steps.skill\|class-module-domain-rule-steps]] |

## What Does NOT Belong Here
- Handler/orchestration scenarios — belong to `{Module}.Application.Tests`.
- A reference to `{Module}.Application` or any infrastructure project.

## Allowed Dependencies
- Reference `{Module}.Domain` and mirror its production project's assembled [Allowed Dependencies](skills/dotnet/architecture/plateau/plateau-domain-service/structure/{Module}.Domain/plateau-domain-service--csproj-module-domain.skill.md#allowed-dependencies); no wider project boundary.

# Rules
MUST:
- Apply the linked [testing conventions](#testing-conventions).
- Apply the mirrored [dependency boundary](#allowed-dependencies), including any explicitly applied architecture-test extension.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]

# Check list
- [ ] References match the mirrored production boundary and any explicitly applied architecture-test extension.
