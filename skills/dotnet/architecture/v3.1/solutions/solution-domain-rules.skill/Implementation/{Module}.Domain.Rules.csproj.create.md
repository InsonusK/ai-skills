---
description: Dedicated, reusable project holding every business predicate (Rule) for a module — referenced by FluentValidation and {Module}.Interfaces only
project_name: "{Module}.Domain.Rules"
name: "{Module}.Domain.Rules.csproj"
element_kind: project
change_kind: create
tags:
  - solution/domain-rules
  - element/module-domain-rules-csproj
---

# Goals
- Give every business predicate for a module one dedicated, reusable project — isolatable for mutation testing without pulling in Entities or other Domain-layer code
- Let another .NET service reuse a rule's condition, unmodified, without adopting this service's `DomainException`/pipeline conventions

# Core Principles
- `{Module}.Domain.Rules` references FluentValidation and `{Module}.Interfaces` — nothing else. No repository, no `DbContext`, no `{Module}.Domain`, no `{Module}.Application`.
- This project is the module's fifth project, added on top of the base four (`Api`/`Application`/`Domain`/`Interfaces`) established by `solution-sln-structure`, because Rules needs project-level isolation the base four cannot provide — see that solution's own `module-project-set-extensibility` ADR.

# Implementation changes

```
/src/Modules/{ModuleName}
  /{ModuleName}.Domain.Rules
    /Common
      ModuleInfo.cs
    {Rule}.cs
```

# Rule changes

## MUST
- Reference `FluentValidation` and `{Module}.Interfaces` only
- Live under `/src/Modules/{ModuleName}/{ModuleName}.Domain.Rules`
- Never reference a repository, `DbContext`, `{Module}.Domain`, or `{Module}.Application`
- Never perform any I/O

# Check list
- [ ] `{Module}.Domain.Rules.csproj` references only FluentValidation and `{Module}.Interfaces`
- [ ] No repository/`DbContext`/`{Module}.Domain`/`{Module}.Application` reference anywhere in the project
