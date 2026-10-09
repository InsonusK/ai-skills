---
version: 20261009220001
description: Dedicated test project for {Module}.Domain.Rules — proves every rule's own IsValid()/Check()/IRuleBuilder extension, isolated from the broader Entity/VO mutation surface of {Module}.Domain.Tests
project_name: "{Module}.Domain.Rules.Tests"
name: "{Module}.Domain.Rules.Tests.csproj"
element_kind: project
change_kind: create
tags:
  - solution/domain-shared-rules
  - element/module-domain-rules-tests-csproj
---

# Goals
- Give `{Module}.Domain.Rules` its own dedicated test project, mirroring the one-test-project-per-production-project pattern `solution-dotnet-conformance-testing` already establishes for the base five projects
- Isolate `{Module}.Domain.Rules`'s mutation-testing surface from `{Module}.Domain.Tests`'s broader one (which also covers Entities/VOs) — a survived mutant here is unambiguously a rule bug, not noise from an unrelated Entity method

# Core Principles
- References `{Module}.Domain.Rules` and mirrors its [Allowed Dependencies](skills/dotnet/architecture/solutions/solution-domain-shared-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create.md#allowed-dependencies).
- Takes `.feature` files from two sources: its own `/features` folder (rule-only edge cases no other layer needs to prove) and, linked in as `<ReqnrollFeatureFiles>`, every file under every classification folder of `{Module}.Domain.Rules.Spec` — the shared scenarios also proven by `{Module}.Domain.Tests`/`{Module}.Application.Tests`
- Step definitions here call the rule's own `Check()` (or the raw `IsValid()` for a pure-predicate scenario) directly — never a VO constructor, an Entity method, or a validator; those adapters are proven in their own test projects

# Implementation changes

Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for project contents and tooling.
The optional local features cover rule-only edge cases; the shared spec is linked as follows.

`{Module}.Domain.Rules.Tests.csproj` links the shared spec directory in:

```xml
<ItemGroup>
  <ReqnrollFeatureFiles Include="..\{ModuleName}.Domain.Rules.Spec\**\*.feature" Link="features\Shared\%(RecursiveDir)%(Filename)%(Extension)" />
</ItemGroup>

<ItemGroup>
  <ProjectReference Include="..\{ModuleName}.Domain.Rules\{ModuleName}.Domain.Rules.csproj" />
</ItemGroup>
```

Reqnroll generates a fixture only for `ReqnrollFeatureFiles` items — a file linked as `<None>` builds cleanly but produces no test, so its scenarios silently never run. The `Link` metadata only changes where the file shows up in the project, not how the build treats it. Every scenario, regardless of tag, is in scope here — this project proves the rule itself, not one adapter.

# Rule changes

## MUST
- Mirror the tested production project's Allowed Dependencies, plus `{Module}.Domain.Rules` itself.
- Link the entire `{Module}.Domain.Rules.Spec` directory in as `<ReqnrollFeatureFiles Include>` — never `<None Include>`, which generates no test — not copy scenario text into this project's own `.feature` files
- Step definitions call `{Rule}.Check()`/`.IsValid()` directly, never a VO/Entity/validator adapter
- Domain/Application entry points are outside the rule behavior being proven; if the Cecil companion is applied, its inspected-assembly references are defined by [the architecture-test extension](skills/dotnet/architecture/solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.extend.md).
- Never duplicate a scenario already present in `{Module}.Domain.Rules.Spec` inside this project's own `/features` folder

# Check list
- [ ] The rule-test reference boundary mirrors Domain.Rules; any architecture-scan references come from the applied Cecil extension.
- [ ] `{Module}.Domain.Rules.Spec/**/*.feature` is linked in as `<ReqnrollFeatureFiles Include>`, and the `result/scenarios.json` inventory lists none of its scenarios as `not-run`
- [ ] Every scenario in the linked spec has a passing step-definition binding here, regardless of classification tag
