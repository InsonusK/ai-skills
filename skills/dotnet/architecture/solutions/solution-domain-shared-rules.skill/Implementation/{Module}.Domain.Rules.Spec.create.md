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
- One `.feature` file per rule class and classification, named after the rule (`{Rule}.feature` for `{Rule}Rules`/`{Rule}Rule`), in the folder of its classification: `format/`, `semantic/`, or `domain/`. A rule reused at more than one layer gets one file per folder, not one file with mixed tags
- Every scenario carries exactly one classification tag — `@format`, `@semantic`, or `@domain` — equal to its folder. The folder is what a consuming project links; the tag keeps the classification visible to a reader and to an external consumer that copies the files
- A consuming test project links the folders it proves in via its own `.csproj` as `<ReqnrollFeatureFiles Include="..\{ModuleName}.Domain.Rules.Spec\format\**\*.feature" Link="..." />` — never `<None Include>`, for which Reqnroll generates no test at all. Linking by folder is the filter: Reqnroll generates a test for every scenario of every file it is given, so a project never receives a file it has no step definitions for (see [[skills/dotnet/architecture/solutions/solution-domain-shared-rules.skill/adr/spec-folders-per-classification|ADR]]) — see [[skills/dotnet/architecture/solutions/solution-domain-shared-rules.skill/Implementation/{Module}.Domain.Rules.Tests.csproj.create|{Module}.Domain.Rules.Tests.csproj]], [[skills/dotnet/architecture/solutions/solution-domain-shared-rules.skill/Implementation/{Module}.Domain.Tests.csproj.extend|{Module}.Domain.Tests.csproj]], [[skills/dotnet/architecture/solutions/solution-domain-shared-rules.skill/Implementation/{Module}.Application.Tests.csproj.extend|{Module}.Application.Tests.csproj]]

# Implementation changes

```
/src/Modules/{ModuleName}
  /{ModuleName}.Domain.Rules
  /{ModuleName}.Domain.Rules.Spec
    /format
      {Rule}.feature
    /semantic
      {Rule}.feature
    /domain
      {Rule}.feature
  /{ModuleName}.Domain.Rules.Tests
```

See [[skills/dotnet/architecture/solutions/solution-domain-shared-rules.skill/Implementation/{Module}.Domain.Rules.Spec.create/{Rule}.feature.create|{Rule}.feature]] for worked `.feature` examples.

# Rule changes

## MUST
- Contain only `.feature` files, one per rule class and classification, inside `format/`, `semantic/`, or `domain/`
- Every scenario carry exactly one of `@format`/`@semantic`/`@domain`, equal to the folder its file is in
- Every scenario be written in domain language only — no .NET type name, no C#/FluentValidation term, no reference to the adapter that proves it — so the file stays copyable into a consumer repo in another language; rejection codes appear verbatim
- Live at `/src/Modules/{ModuleName}/{ModuleName}.Domain.Rules.Spec`, as a sibling of `{ModuleName}.Domain.Rules`, not nested inside it
- Never contain a `.csproj`, a `.cs` file, or any step definition
- Never be referenced as a project by any other `.csproj` — only individual `.feature` files are linked in by path

# Check list
- [ ] Directory contains only `.feature` files, no code, no project file
- [ ] Every scenario has exactly one classification tag, matching its folder
- [ ] Every scenario is domain language only — no .NET types or adapter references — and stays portable to another language; rejection codes verbatim
- [ ] File name matches the rule class it describes; the file lives in `format/`, `semantic/`, or `domain/`
