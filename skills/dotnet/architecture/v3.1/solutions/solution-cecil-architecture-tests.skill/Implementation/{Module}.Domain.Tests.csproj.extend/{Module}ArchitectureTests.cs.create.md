---
description: The three single-pass Cecil checks — dead-rule detection, exception-type scoping, generated-constant uniqueness/format — one class, three [Fact]s
project_name: "{Module}.Domain.Tests"
name: "{Module}ArchitectureTests.cs"
element_kind: class
change_kind: create
tags:
  - solution/cecil-architecture-tests
  - element/module-architecture-tests-cs
---

# Goals
- Prove, over compiled IL rather than by executing it, that every `Check()` in `Domain.Rules` is actually called, `DomainException` is only ever thrown from `ValueObjects`/`Entities`, and every generated rejection code is unique and well-formed

# Core Principles
- Single-pass checks (scan every method once, flag) share this one class — the call-graph/registry-driven check does not belong here, see [[skills/dotnet/architecture/v3.1/solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/GuardedPropertyRuleCoverageTests.cs.create.md|GuardedPropertyRuleCoverageTests.cs]]
- Load every assembly via `typeof(KnownType).Assembly.Location`, never a hardcoded path

# Implementation changes

Full worked implementation (`TaskModule`) and the "why" behind each of `Code.Call`/`Code.Callvirt` matching, `Code.Newobj` scoping, and `f.HasConstant` field reading:
- [[skills/dotnet/architecture/v3.1/solutions/solution-cecil-architecture-tests.skill/examples/dead-rule-detection.md|dead-rule-detection.md]] — `EveryDomainRuleCheck_IsCalledByProductionCodeOutsideRules`
- [[skills/dotnet/architecture/v3.1/solutions/solution-cecil-architecture-tests.skill/examples/exception-scoping.md|exception-scoping.md]] — `DomainException_IsThrownOnlyFromValueObjectsOrEntities`
- [[skills/dotnet/architecture/v3.1/solutions/solution-cecil-architecture-tests.skill/examples/code-uniqueness-format.md|code-uniqueness-format.md]] — `RejectionCodes_AreUniqueAndFollowModuleDotClassDotReasonFormat`

All three `[Fact]`s live in one `{Module}ArchitectureTests` class in `/Architecture/{Module}ArchitectureTests.cs`, sharing the `LoadDomainAssembly()`/`LoadDomainRulesAssembly()` helpers shown in the linked examples.

# Rule changes

## MUST
- Contain exactly these three `[Fact]`s, no more, no fewer, per module
- Load assemblies via `typeof(KnownType).Assembly.Location`
- Never contain the registry-driven call-graph check — that is a separate class, see `GuardedPropertyRuleCoverageTests.cs`

# Check list
- [ ] All three checks present, each loading its target assembly correctly
- [ ] No hardcoded assembly path anywhere in this file
