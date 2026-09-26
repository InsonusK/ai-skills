---
name: solution-dotnet-conformance-testing
description: The dotnet plateau catalog's test-project layout — one test project per production project, mirroring its Allowed Dependencies exactly, each holding unit tests, Reqnroll feature files, and step definitions together; the test tooling itself comes from solution-conformance-testing-in-dotnet
whenToUse: Decide which test project a scenario or unit test belongs in, create the test project for a production project of the dotnet plateau catalog, or review whether a test project's references mirror its production project's Allowed Dependencies.
domain: skill
type: architecture
version: 20260924000000
tags:
  - skill/architecture/solution
  - solution/dotnet-conformance-testing
  - stack/dotnet
  - concern/testing
  - concern/testing/bdd
  - cucumber
  - concern/architecture
creates:
  - "{Module}.Domain.Tests.csproj"
  - "{Module}.Application.Tests.csproj"
  - "{Module}.Interfaces.Tests.csproj"
  - Shared.Tests.csproj
  - BuildingBlocks.Tests.csproj
  - "{TestProject}.StepDefinitions.{Rule}Steps.cs"
extends:
depends_on:
  - "[[skills/dotnet/test/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md|solution-conformance-testing-in-dotnet]]"
built_on_plateau:
adr:
  - "[[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/adr/test-project-per-production-project|One test project per production project, not per module]]"
  - "[[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/adr/tooling-moved-to-stack-testing-skill|Test tooling moved to solution-conformance-testing-in-dotnet]]"
---

# Goal
- Give every testable .NET production project that exists — `{Module}.Application`, `{Module}.Interfaces`, `Shared`, `BuildingBlocks`, plus `{Module}.Domain` once VP1 is applied — its own test project, so a reader can always answer "what does this test project reference and prove" by looking at exactly one production project.

# Capabilities
- Every production project's Allowed Dependencies rule (from `solution-sln-structure`) has a matching test project with the same, mirrored dependency: `{Module}.Domain.Tests` references only `{Module}.Domain`, `{Module}.Interfaces.Tests` references only `{Module}.Interfaces`, and so on.
- Gherkin `.feature` files execute against real production code via Reqnroll step definitions, in whichever test project owns the code being proven.
- Every test project is picked up by the `make unit-test`/`mutation-test`/`test-report` contract of [[skills/dotnet/test/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md|solution-conformance-testing-in-dotnet]] without any per-project wiring.

# Core Principles
- One test project per production project **that exists**, never one combined project per module. At the v3.1 baseline that is `{Module}.Application.Tests`, `{Module}.Interfaces.Tests`, `Shared.Tests`, `BuildingBlocks.Tests`. `{Module}.Domain.Tests` appears only when the module has a domain layer (`solution-domain-behaviour`, VP1); `{Module}.Domain.Rules.Tests` only with VP4. `{Module}.Api` has no dedicated test project — it is a thin MediatR adapter with no business logic of its own to prove (see [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/adr/test-project-per-production-project|ADR]]).
- Each test project's Allowed Dependencies mirror its production counterpart's exactly: `{Module}.Application.Tests` may reference `{Module}.Application` and `{Module}.Domain` (the same two `{Module}.Application.csproj` itself is allowed to reference), `BuildingBlocks.Tests` may reference `BuildingBlocks` and `Shared`, and so on. A test project never reaches further than the production project it tests is itself allowed to reach.
- Every test project contains unit tests, Reqnroll feature files, and their step definitions together — never a separate project split out just for Gherkin scenarios.
- Step definitions call the tested project's real public API; they never re-implement the rule under test.
- The `Makefile`, scripts, `reqnroll.json`, and report are owned by [[skills/dotnet/test/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md|solution-conformance-testing-in-dotnet]]; this solution only decides the test-project layout.

# Adr
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/adr/test-project-per-production-project|One test project per production project, not per module]]
  - Selected variant: one test project per production project, mirroring its Allowed Dependencies
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/adr/tooling-moved-to-stack-testing-skill|Test tooling moved to solution-conformance-testing-in-dotnet]]
  - Selected variant: keep only the test-project layout here; the stack-generic tooling lives in `skills/dotnet/test/`

# Requirements
SOLUTION:
- [[skills/dotnet/test/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md|solution-conformance-testing-in-dotnet]]
  - Provides the Reqnroll/coverlet/Stryker.NET tooling and the `make unit-test`/`mutation-test`/`test-report`/`test-and-report` contract that runs every test project created here.
- [[skills/dotnet/architecture/solutions/solution-sln-structure.skill/solution-sln-structure.skill|solution-sln-structure]]
  - Defines the production projects (`{Module}.Domain`, `{Module}.Application`, `{Module}.Interfaces`, `Shared`, `BuildingBlocks`) and their Allowed Dependencies, which each test project here mirrors.

NUGET:
- Reqnroll.xUnit, coverlet.collector, Microsoft.NET.Test.Sdk — referenced by every test project; chosen in [[skills/dotnet/test/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md|solution-conformance-testing-in-dotnet]]'s own ADR.

# Template Skill Mutations
PROJECT:
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create|{Module}.Domain.Tests.csproj]] - create - tests `{Module}.Domain` only
  - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]] - create - validator-shaped step definitions: input → valid/invalid + error code
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create|{Module}.Application.Tests.csproj]] - create - tests `{Module}.Application` (and, transitively, `{Module}.Domain`)
  - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]] - create - command-shaped step definitions: command → handler → `Result`
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create|{Module}.Interfaces.Tests.csproj]] - create - tests `{Module}.Interfaces` only
  - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]] - create - shape-shaped step definitions: equality/serialization round-trip
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/Shared.Tests.csproj.create|Shared.Tests.csproj]] - create - tests `Shared` only
  - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/Shared.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]] - create - value-shaped step definitions: primitive comparison/combination
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create|BuildingBlocks.Tests.csproj]] - create - tests `BuildingBlocks` (and, transitively, `Shared`)
  - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]] - create - technical-contract-shaped step definitions: pipeline behavior's observable contract

# Workflow
## Add conformance coverage for a new validation rule (happy path)
1. Decide which production project owns the rule (e.g. an entity invariant lives in `{Module}.Domain`, a transport check lives in `{Module}.Application`).
2. A `.feature` file describing the rule (e.g. `Rules/{Rule}.feature`) is added or extended, inside that project's own test project (e.g. `{Module}.Domain.Tests/Rules/{Rule}.feature`), with `Given/When/Then` scenarios.
3. `{Rule}Steps.cs` is created in that same test project with `[Given]`/`[When]`/`[Then]` bindings that call the real production code.
4. The gate runs as described in [[skills/dotnet/test/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md|solution-conformance-testing-in-dotnet]]'s own Workflow.

# Rules
Each linked `#MUST` section below carries its own `Violation`/`Risk`/`Fix` at the target — this index only points to where the actual rule lives.

## MUST
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create#MUST|{Module}.Domain.Tests.csproj]]
  - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create/{Rule}Steps.cs.create#MUST|{Rule}Steps.cs]]
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create#MUST|{Module}.Application.Tests.csproj]]
  - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create#MUST|{Rule}Steps.cs]]
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create#MUST|{Module}.Interfaces.Tests.csproj]]
  - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create/{Rule}Steps.cs.create#MUST|{Rule}Steps.cs]]
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/Shared.Tests.csproj.create#MUST|Shared.Tests.csproj]]
  - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/Shared.Tests.csproj.create/{Rule}Steps.cs.create#MUST|{Rule}Steps.cs]]
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create#MUST|BuildingBlocks.Tests.csproj]]
  - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create/{Rule}Steps.cs.create#MUST|{Rule}Steps.cs]]
- Give every test project exactly the same Allowed Dependencies as the one production project it tests — never wider.
  - Risk: a test project that references more than its production counterpart is allowed to (e.g. `{Module}.Domain.Tests` referencing `{Module}.Application`) can pass by exercising code its own production project could never legally reach, hiding a real dependency violation.
  - Fix: mirror each production project's own Allowed Dependencies list exactly when scoping its test project.

# Check list
- [ ] Every production project that exists has exactly one test project (`{Module}.Domain.Tests` only with VP1).
- [ ] Each test project's references match its production counterpart's Allowed Dependencies exactly — no wider, no narrower.
- [ ] `{Module}.Api` has no dedicated test project.
- [ ] Every `.feature` scenario has a matching step definition, in the same test project as the code it proves, that calls production code.
- [ ] [[skills/dotnet/test/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md|solution-conformance-testing-in-dotnet]] is applied, so `make unit-test` picks up every test project.
