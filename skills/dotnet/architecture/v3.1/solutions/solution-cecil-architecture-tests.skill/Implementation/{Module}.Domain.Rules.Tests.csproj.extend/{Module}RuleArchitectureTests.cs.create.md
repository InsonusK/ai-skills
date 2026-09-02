---
description: The two single-pass Cecil checks that scan {Module}.Domain.Rules only — dead-rule detection and generated-constant uniqueness/format — one class, two [Fact]s
project_name: "{Module}.Domain.Rules.Tests"
name: "{Module}RuleArchitectureTests.cs"
element_kind: class
change_kind: create
tags:
  - solution/cecil-architecture-tests
  - element/module-rule-architecture-tests-cs
---

# Goals
- Prove, over compiled IL, that every `Check()` in `{Module}.Domain.Rules` is called by production code outside the Rules assembly, and that every generated rejection code is unique and matches `{ModuleName}.{Class}.{Reason}`.
- Run for any module that has VP4, whether or not it has a domain layer — hence this project, not `{Module}.Domain.Tests`.

# Core Principles
- Both checks are single-pass (scan every method / field once, flag) — they share this one class and the `LoadDomainRulesAssembly()` / `LoadProductionAssemblies()` helpers.
- Load every assembly via `typeof(KnownType).Assembly.Location`, never a hardcoded path.
- The dead-rule check inspects `{Module}.Domain` + `{Module}.Application` (the production assemblies that should call `Check()`); when the module has no domain layer it inspects `{Module}.Application` alone.

# Implementation changes

Two `[Fact]`s in one `{Module}RuleArchitectureTests` class in `{Module}.Domain.Rules.Tests/Architecture/{Module}RuleArchitectureTests.cs`:

- `EveryDomainRuleCheck_IsCalledByProductionCodeOutsideRules` — collect every `Check()` / `IRuleBuilder`-extension method in `{Module}.Domain.Rules`; collect every `Code.Call`/`Code.Callvirt` target across every method body in the production assemblies; fail if any entry point is never called. Full worked implementation and rationale: [[skills/dotnet/architecture/v3.1/solutions/solution-cecil-architecture-tests.skill/examples/dead-rule-detection.md|dead-rule-detection.md]].
- `RejectionCodes_AreUniqueAndFollowModuleDotClassDotReasonFormat` — collect every `f.HasConstant` `string` field named `*Code` in `{Module}.Domain.Rules`; assert each matches the `{ModuleName}\.[A-Za-z0-9]+\.[A-Za-z0-9]+` regex and the set has no duplicates. Full worked implementation: [[skills/dotnet/architecture/v3.1/solutions/solution-cecil-architecture-tests.skill/examples/code-uniqueness-format.md|code-uniqueness-format.md]].

# Rule changes

## MUST
- Contain exactly these two `[Fact]`s, no more, no fewer, per module.
- Load assemblies via `typeof(KnownType).Assembly.Location`.
- Never contain the exception-scoping or guarded-property-coverage checks — those need `{Module}.Domain` and live in `{Module}ArchitectureTests` / `GuardedPropertyRuleCoverageTests` in `{Module}.Domain.Tests`.

# Check list
- [ ] Exactly the two `[Fact]`s (`EveryDomainRuleCheck_IsCalledByProductionCodeOutsideRules`, `RejectionCodes_AreUniqueAndFollowModuleDotClassDotReasonFormat`).
- [ ] Assemblies loaded via `typeof(...).Assembly.Location`; no hardcoded path.
- [ ] No exception-scoping / guarded-property logic in this file.
