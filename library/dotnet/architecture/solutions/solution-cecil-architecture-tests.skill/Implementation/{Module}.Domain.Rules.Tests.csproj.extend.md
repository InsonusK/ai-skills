---
description: Add the Architecture/ folder for the two Cecil checks that scan {Module}.Domain.Rules only — they run whenever VP4 is applied, with or without a domain layer
project_name: "{Module}.Domain.Rules.Tests"
name: "{Module}.Domain.Rules.Tests.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/cecil-architecture-tests
  - element/module-domain-rules-tests-csproj
---

# Goals
- Give `{Module}.Domain.Rules.Tests` a build-time guarantee, over compiled IL, that no `Check()` in `{Module}.Domain.Rules` is dead and every rejection-code constant is unique and well-formed — checks that need only the `{Module}.Domain.Rules` assembly and therefore run for a rules-only module too.

# Rule changes

## MUST
- Add `/Architecture` under `{Module}.Domain.Rules.Tests`, holding `{Module}RuleArchitectureTests.cs` and its companion `.feature` file(s).
- Reference `Mono.Cecil`.
- Load the `{Module}.Domain.Rules` assembly and, for the dead-rule check, the production assemblies expected to call `Check()` (`{Module}.Domain`, `{Module}.Application`) via `typeof(KnownType).Assembly.Location` — never a hardcoded path, and never a whole `AppDomain` scan.
- Never put the exception-scoping or guarded-property-coverage checks here — those need `{Module}.Domain` and live in `{Module}.Domain.Tests`.

# Check list
- [ ] `{Module}.Domain.Rules.Tests.csproj` references `Mono.Cecil` and `{Module}.Domain.Rules` (plus the production assemblies the dead-rule check inspects).
- [ ] `/Architecture/{Module}RuleArchitectureTests.cs` exists with exactly the two `[Fact]`s.
