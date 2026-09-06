---
description: Directory holding one shared .feature file per rule — not a project, nothing compiled, nothing referenced. Every test project that proves a scenario from it links the physical file in and generates its own Reqnroll fixture.
project_name: "{Module}.Domain.Rules.Spec"
name: "{Module}.Domain.Rules.Spec"
element_kind: directory
change_kind: create
tags:
  - solution/domain-shared-rules
  - element/module-domain-rules-spec
---

# Goals
- Give a rule exactly one Gherkin source, provable from every layer that redirects to it (the rule itself, the VO/Entity fail-fast adapter, the DtoValidator collect-all adapter), without writing the same scenario text three times
- Make Format/Semantic/Domain classification visible directly in the `.feature` file, via scenario tags, instead of only in prose
- Serve as an exportable, language-neutral contract: the `.feature` files can be copied into a separate repository (a frontend, or a non-.NET service) that must apply the same rules and proves them there with its own step definitions

# Core Principles
- This is a plain directory, sibling to `{Module}.Domain.Rules` under `/src/Modules/{ModuleName}/`, not a `.csproj` — it produces no assembly and is never referenced by anything
- It contains only `.feature` files — no `.cs`, no step definitions, no `csproj`. Step definitions live in whichever test project proves a given scenario, never here
- Every scenario is written in domain language — no .NET/C#/FluentValidation vocabulary, no type names, no mention of the adapter that proves it — so a consumer in another language binds the same file with its own step definitions without editing the Gherkin. Rejection-code strings are part of the contract and appear verbatim
- One `.feature` file per rule class, named after the rule (`{Rule}.feature` for `{Rule}Rules`/`{Rule}Rule`)
- Every scenario carries exactly one classification tag: `@format`, `@semantic`, or `@domain` — the same classification the rule itself already has in `{Module}.Domain.Rules`. A rule reused at more than one layer gets one scenario per layer, not one scenario claimed to cover both
- A consuming test project links the physical file in via its own `.csproj` (`<None Include="..\{ModuleName}.Domain.Rules.Spec\**\*.feature" Link="..." />`), filtered to the tags that project proves — see [[skills/dotnet/architecture/v3.1/solutions/solution-domain-shared-rules.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.create.md|{Module}.Domain.Rules.Tests.csproj]], [[skills/dotnet/architecture/v3.1/solutions/solution-domain-shared-rules.skill/Implementation/{Module}.Domain.Tests.csproj.extend.md|{Module}.Domain.Tests.csproj]], [[skills/dotnet/architecture/v3.1/solutions/solution-domain-shared-rules.skill/Implementation/{Module}.Application.Tests.csproj.extend.md|{Module}.Application.Tests.csproj]]

# Implementation changes

```
/src/Modules/{ModuleName}
  /{ModuleName}.Domain.Rules
  /{ModuleName}.Domain.Rules.Spec
    {Rule}.feature
  /{ModuleName}.Domain.Rules.Tests
```

See [[skills/dotnet/architecture/v3.1/solutions/solution-domain-shared-rules.skill/Implementation/{Module}.Domain.Rules.Spec.create/{Rule}.feature.create.md|{Rule}.feature]] for worked `.feature` examples.

# Rule changes

## MUST
- Contain only `.feature` files, one per rule class
- Every scenario carry exactly one of `@format`/`@semantic`/`@domain`
- Every scenario be written in domain language only — no .NET type name, no C#/FluentValidation term, no reference to the adapter that proves it — so the file stays copyable into a consumer repo in another language; rejection codes appear verbatim
- Live at `/src/Modules/{ModuleName}/{ModuleName}.Domain.Rules.Spec`, as a sibling of `{ModuleName}.Domain.Rules`, not nested inside it
- Never contain a `.csproj`, a `.cs` file, or any step definition
- Never be referenced as a project by any other `.csproj` — only individual `.feature` files are linked in by path

# Check list
- [ ] Directory contains only `.feature` files, no code, no project file
- [ ] Every scenario has exactly one classification tag
- [ ] Every scenario is domain language only — no .NET types or adapter references — and stays portable to another language; rejection codes verbatim
- [ ] File name matches the rule class it describes
