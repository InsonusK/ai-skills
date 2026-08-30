---
name: plateau-shared-rules--class-module-architecture-tests
description: Class {Module}ArchitectureTests in the shared-rules plateau — the three single-pass Cecil checks
whenToUse: when verifying, over compiled IL, that every centralized rule is actually called, DomainException is thrown only from the right layer, and rejection codes are unique and well-formed
domain: skill
type: template
plateau: shared-rules
version: 20260824150000
tags:
  - skill/template/class
  - plateau/shared-rules
created_by:
  - "[[../../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]]"
---

# Goal
- Prove, over compiled IL rather than by executing it, that every `Check()` in `Domain.Rules` is actually called, `DomainException` is only ever thrown from `ValueObjects`/`Entities`, and every generated rejection code is unique and well-formed

__Applied solutions:__
- [[../../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] - [[../../../../../solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/{Module}ArchitectureTests.cs.create.md|{Module}ArchitectureTests.cs.create]]

# Core Principles
- Load every assembly via `typeof(KnownType).Assembly.Location`, never a hardcoded path
- Match call targets by simple name (`DeclaringType.Name`+`Name`), never full cross-assembly `Resolve()`
- Single-pass checks (scan every method once, flag) share this one class — the recursive call-graph check does not belong here, see [[./plateau-shared-rules--class-guarded-property-rule-coverage-tests.skill.md|class-guarded-property-rule-coverage-tests]]

# Implementation
```csharp
//Skill: class-module-architecture-tests
//Plateau: shared-rules
//Version: 20260824150000

public sealed class {Module}ArchitectureTests
{
    private const string RulesNamespace = "{Module}.Domain.Rules";
    private const string ValueObjectsNamespace = "{Module}.Domain.ValueObjects";
    private const string EntitiesNamespace = "{Module}.Domain.Entities";
    private const string ValidationResultTypeName = "FluentValidation.Results.ValidationResult";
    private static readonly Regex RejectionCodeFormat = new(@"^[A-Za-z]+\.[A-Za-z]+\.[A-Za-z]+$", RegexOptions.Compiled);

    private static AssemblyDefinition LoadDomainAssembly()
        => AssemblyDefinition.ReadAssembly(typeof({Entity}).Assembly.Location);

    private static AssemblyDefinition LoadDomainRulesAssembly()
        => AssemblyDefinition.ReadAssembly(typeof({Rule}Rules).Assembly.Location);

    [Fact]
    public void EveryDomainRuleCheck_IsCalledByProductionCodeOutsideRules() { /* see solution-cecil-architecture-tests examples/dead-rule-detection.md */ }

    [Fact]
    public void DomainException_IsThrownOnlyFromValueObjectsOrEntities() { /* see solution-cecil-architecture-tests examples/exception-scoping.md */ }

    [Fact]
    public void RejectionCodes_AreUniqueAndFollowModuleDotClassDotReasonFormat() { /* see solution-cecil-architecture-tests examples/code-uniqueness-format.md */ }
}
```

Full worked implementation of all three `[Fact]`s (the exact `Code.Call`/`Code.Newobj`/`f.HasConstant` walks) and the detailed rationale for each: [[../../../../../solutions/solution-cecil-architecture-tests.skill/examples/dead-rule-detection.md|dead-rule-detection.md]], [[../../../../../solutions/solution-cecil-architecture-tests.skill/examples/exception-scoping.md|exception-scoping.md]], [[../../../../../solutions/solution-cecil-architecture-tests.skill/examples/code-uniqueness-format.md|code-uniqueness-format.md]].

__Applied solutions:__
- [[../../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] - [[../../../../../solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/{Module}ArchitectureTests.cs.create.md|{Module}ArchitectureTests.cs.create]]

# Rules
MUST:
- Contain exactly these three `[Fact]`s, no more, no fewer, per module
- Load assemblies via `typeof(KnownType).Assembly.Location`
MUST NOT:
- Contain the registry-driven call-graph check — that lives in `GuardedPropertyRuleCoverageTests`

__Applied solutions:__
- [[../../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] - [[../../../../../solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/{Module}ArchitectureTests.cs.create.md|{Module}ArchitectureTests.cs.create]]

# Check list
- [ ] All three checks present, each loading its target assembly correctly
- [ ] No hardcoded assembly path anywhere in this file

__Applied solutions:__
- [[../../../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]] - [[../../../../../solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/{Module}ArchitectureTests.cs.create.md|{Module}ArchitectureTests.cs.create]]
