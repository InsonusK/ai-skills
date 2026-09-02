---
description: The exception-type-scoping Cecil check — DomainException / EntityNotLoadedException are only constructed from the namespace meant to throw them — one [Fact], VP1-gated
project_name: "{Module}.Domain.Tests"
name: "{Module}ArchitectureTests.cs"
element_kind: class
change_kind: create
tags:
  - solution/cecil-architecture-tests
  - element/module-architecture-tests-cs
---

# Goals
- Prove, over compiled IL rather than by executing it, that `DomainException` is constructed only from `{Module}.Domain.ValueObjects` / `{Module}.Domain.Entities`, and `EntityNotLoadedException` only from `{Module}.Domain` — never leaked into `{Module}.Application`, `{Module}.Api`, or a handler.

# Core Principles
- Single-pass check (scan every method body once for `Code.Newobj`, flag). Lives in this one class with the `LoadDomainAssembly()` helper.
- Load every assembly via `typeof(KnownType).Assembly.Location`, never a hardcoded path.
- VP1-gated — needs `{Module}.Domain` and `DomainException` to exist. The dead-rule and code-uniqueness checks (which need only `{Module}.Domain.Rules`) live separately in `{Module}RuleArchitectureTests` in `{Module}.Domain.Rules.Tests`.

# Implementation changes

One `[Fact]` in `{Module}ArchitectureTests` in `{Module}.Domain.Tests/Architecture/{Module}ArchitectureTests.cs`:

- `DomainException_IsThrownOnlyFromValueObjectsOrEntities` — for each exception type and its allowed namespace set, walk every method body outside the allowed namespaces for a `Code.Newobj` whose `DeclaringType.FullName` matches the exception type; fail on any match. Full worked implementation and rationale: [[skills/dotnet/architecture/v3.1/solutions/solution-cecil-architecture-tests.skill/examples/exception-scoping.md|exception-scoping.md]].

# Rule changes

## MUST
- Contain exactly this one `[Fact]`, per module.
- Load assemblies via `typeof(KnownType).Assembly.Location`.
- Never contain the dead-rule or code-uniqueness checks — those scan `{Module}.Domain.Rules` and live in `{Module}RuleArchitectureTests` in `{Module}.Domain.Rules.Tests`.
- Never contain the registry-driven call-graph check — that is `GuardedPropertyRuleCoverageTests`.

# Check list
- [ ] Exactly the one `[Fact]` (`DomainException_IsThrownOnlyFromValueObjectsOrEntities`), covering both `DomainException` and `EntityNotLoadedException`.
- [ ] Assembly loaded via `typeof(...).Assembly.Location`; no hardcoded path.
