---
description: Directory holding one shared .feature file per rule — not a project, nothing compiled, nothing referenced. Every test project that proves a scenario from it links the physical file in and generates its own Reqnroll fixture.
project_name: "{Module}.Domain.Rules.Spec"
name: "{Module}.Domain.Rules.Spec"
element_kind: directory
change_kind: create
tags:
  - solution/domain-rules
  - element/module-domain-rules-spec
---

# Goals
- Give a rule exactly one Gherkin source, provable from every layer that redirects to it (the rule itself, the VO/Entity fail-fast adapter, the DtoValidator collect-all adapter), without writing the same scenario text three times
- Make Format/Semantic/Domain classification visible directly in the `.feature` file, via scenario tags, instead of only in prose

# Core Principles
- This is a plain directory, sibling to `{Module}.Domain.Rules` under `/src/Modules/{ModuleName}/`, not a `.csproj` — it produces no assembly and is never referenced by anything
- It contains only `.feature` files — no `.cs`, no step definitions, no `csproj`. Step definitions live in whichever test project proves a given scenario, never here
- One `.feature` file per rule class, named after the rule (`{Rule}.feature` for `{Rule}Rules`/`{Rule}Rule`)
- Every scenario carries exactly one classification tag: `@format`, `@semantic`, or `@domain` — the same classification the rule itself already has in `{Module}.Domain.Rules`. A rule reused at more than one layer gets one scenario per layer, not one scenario claimed to cover both
- A consuming test project links the physical file in via its own `.csproj` (`<None Include="..\{ModuleName}.Domain.Rules.Spec\**\*.feature" Link="..." />`), filtered to the tags that project proves — see [[skills/dotnet/architecture/draft/solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.create|{Module}.Domain.Rules.Tests.csproj]], [[skills/dotnet/architecture/draft/solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Tests.csproj.extend|{Module}.Domain.Tests.csproj]], [[skills/dotnet/architecture/draft/solutions/solution-domain-rules.skill/Implementation/{Module}.Application.Tests.csproj.extend|{Module}.Application.Tests.csproj]]

# Implementation changes

```
/src/Modules/{ModuleName}
  /{ModuleName}.Domain.Rules
  /{ModuleName}.Domain.Rules.Spec
    {Rule}.feature
  /{ModuleName}.Domain.Rules.Tests
```

See [[skills/dotnet/architecture/draft/solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.Spec.create/{Rule}.feature.create|{Rule}.feature]] for worked `.feature` examples.

# Rule changes

## MUST
- Contain only `.feature` files, one per rule class
- Every scenario carry exactly one of `@format`/`@semantic`/`@domain`
- Live at `/src/Modules/{ModuleName}/{ModuleName}.Domain.Rules.Spec`, as a sibling of `{ModuleName}.Domain.Rules`, not nested inside it

## MUST NOT
- Contain a `.csproj`, a `.cs` file, or any step definition
- Be referenced as a project by any other `.csproj` — only individual `.feature` files are linked in by path

# Check list
- [ ] Directory contains only `.feature` files, no code, no project file
- [ ] Every scenario has exactly one classification tag
- [ ] File name matches the rule class it describes
