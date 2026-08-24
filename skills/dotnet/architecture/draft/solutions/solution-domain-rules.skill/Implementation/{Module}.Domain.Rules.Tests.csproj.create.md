---
description: Dedicated test project for {Module}.Domain.Rules — proves every rule's own IsValid()/Check()/IRuleBuilder extension, isolated from the broader Entity/VO mutation surface of {Module}.Domain.Tests
project_name: "{Module}.Domain.Rules.Tests"
name: "{Module}.Domain.Rules.Tests.csproj"
element_kind: project
change_kind: create
tags:
  - solution/domain-rules
  - element/module-domain-rules-tests-csproj
---

# Goals
- Give `{Module}.Domain.Rules` its own dedicated test project, mirroring the one-test-project-per-production-project pattern `solution-conformance-testing` already establishes for the base five projects
- Isolate `{Module}.Domain.Rules`'s mutation-testing surface from `{Module}.Domain.Tests`'s broader one (which also covers Entities/VOs) — a survived mutant here is unambiguously a rule bug, not noise from an unrelated Entity method

# Core Principles
- References `{Module}.Domain.Rules` only — mirrors `{Module}.Domain.Rules.csproj`'s own zero project references (plus FluentValidation/`{Module}.Interfaces`, already transitive through it)
- Takes `.feature` files from two sources: its own `/Rules` folder (rule-only edge cases no other layer needs to prove) and, linked in via `<None Include>`, every file under `{Module}.Domain.Rules.Spec` — the shared scenarios also proven by `{Module}.Domain.Tests`/`{Module}.Application.Tests`
- Step definitions here call the rule's own `Check()` (or the raw `IsValid()` for a pure-predicate scenario) directly — never a VO constructor, an Entity method, or a validator; those adapters are proven in their own test projects

# Implementation changes

```
/src/Modules/{ModuleName}
  /{ModuleName}.Domain.Rules.Tests
    /Rules
      {Rule}.feature          (rule-only scenarios, not shared with other layers)
    /StepDefinitions
      {Rule}RuleSteps.cs
    {ModuleName}.Domain.Rules.Tests.csproj
```

`{Module}.Domain.Rules.Tests.csproj` links the shared spec directory in:

```xml
<ItemGroup>
  <None Include="..\{ModuleName}.Domain.Rules.Spec\**\*.feature" Link="Rules\Shared\%(RecursiveDir)%(Filename)%(Extension)" />
</ItemGroup>

<ItemGroup>
  <ProjectReference Include="..\{ModuleName}.Domain.Rules\{ModuleName}.Domain.Rules.csproj" />
</ItemGroup>
```

Reqnroll generates a fixture from every linked `.feature` file the same way it would for one physically inside the project — the `Link` metadata only changes where Solution Explorer shows it, not how the build treats it. Every scenario, regardless of tag, is in scope here — this project proves the rule itself, not one adapter.

# Rule changes

## MUST
- Reference `{Module}.Domain.Rules` and nothing else
- Link the entire `{Module}.Domain.Rules.Spec` directory in via `<None Include>`, not copy scenario text into this project's own `.feature` files
- Step definitions call `{Rule}.Check()`/`.IsValid()` directly, never a VO/Entity/validator adapter

## MUST NOT
- Add a project reference to `{Module}.Domain`, `{Module}.Application`, or any other module project
- Duplicate a scenario already present in `{Module}.Domain.Rules.Spec` inside this project's own `/Rules` folder

# Check list
- [ ] `{Module}.Domain.Rules.Tests.csproj` references `{Module}.Domain.Rules` only
- [ ] `{Module}.Domain.Rules.Spec/**/*.feature` is linked in via `<None Include>`
- [ ] Every scenario in the linked spec has a passing step-definition binding here, regardless of classification tag
