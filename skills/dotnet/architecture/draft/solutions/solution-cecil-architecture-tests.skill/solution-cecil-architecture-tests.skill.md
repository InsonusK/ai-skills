---
name: solution-cecil-architecture-tests
description: Four Mono.Cecil-based architecture tests that verify structural facts about compiled .NET production code — that a rule/predicate is actually wired in (not dead), that an exception type is only thrown from the layer meant to throw it, that generated codes/constants stay unique and correctly formatted, and that every entity member writing a rule-guarded property also calls that rule.
whenToUse: When a .NET module needs a build-time guarantee that plain xUnit/Reqnroll tests cannot give by construction — a rule/predicate is dead code, an exception type leaks out of its intended layer, a generated code/constant collides or is malformed, or an Entity method/setter/constructor writes a property a Semantic/Domain rule is supposed to guard without calling that rule. Also when deciding whether a new invariant needs a bespoke Cecil test or fits one of the four existing ones.
domain: skill
type: architecture
version: 20260817
tags:
  - skill/architecture/solution
  - dotnet
  - testing
  - cecil
  - architecture-test
creates:
  - "{Module}.Domain.Tests.Architecture.{Module}ArchitectureTests.cs"
  - "{Module}.Domain.Tests.Architecture.GuardedPropertyRuleCoverageTests.cs"
  - "{Module}.Domain.Tests.Architecture.{Check}.feature"
depends_on:
  - "[[skills/dotnet/architecture/draft/solutions/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]]"
built_on_plateau: "[[skills/dotnet/architecture/draft/plateau/plateau-stateless-non-interactive-service/plateau-stateless-non-interactive-service.skill/plateau-stateless-non-interactive-service.skill.md|plateau-stateless-non-interactive-service]]"
adr:
  - "[[./adr/cecil-over-reflection.md|Mono.Cecil over Reflection/Roslyn for architecture tests]]"
  - "[[./adr/registry-driven-coverage-over-per-rule-tests.md|Registry-driven coverage check over one test per rule]]"
---

# Goal

- Prove structural facts about production code — is this dead, is this thrown from the right place, are these constants well-formed and unique, is this write always paired with its rule — that unit/BDD tests, which only exercise the paths their author thought to write, cannot prove by construction.
- Give a rule author exactly one place to register a new invariant (a registry entry, a namespace/type-name constant) instead of writing a new bespoke test class per rule.

# Capabilities

- Dead-rule detection: every `Check()` in `Domain.Rules` must be called by production code outside it.
- Exception-type scoping: a given exception type is only constructed from the namespaces meant to throw it.
- Generated-constant uniqueness/format: every code/constant matching a naming convention is unique and well-formed.
- Guarded-property rule coverage: every public/internal Entity method/setter/init/constructor that writes a property a multi-field rule guards also calls that rule, directly or through a private/internal helper — registry-driven, one line per new rule, no new test per rule.

# Core Principles

- Read compiled IL via Mono.Cecil, don't execute it — a check over `MethodDefinition.Body.Instructions` is a fact about what the shipped code does, not a black-box assertion about what it returns for one input. See [[./adr/cecil-over-reflection.md|the ADR]] for why Cecil, not Reflection or a Roslyn analyzer.
- Load the same already-built assembly other tests in the project reference, via `typeof(KnownType).Assembly.Location` — never a hardcoded or re-derived path.
- Match call targets by simple name (`DeclaringType.Name` + `Name`), not full cross-assembly symbol resolution — the calling assembly's own metadata already carries what a name-based match needs, without a `Resolve()` that can fail if the target assembly's model can't be loaded.
- A coverage registry (`Dictionary<(Entity, Property), Rule[]>`) belongs in the test project, never in production code — it is verification metadata; putting it in `Domain`/`Domain.Rules` would create a dependency a portable Rules project must not carry. See [[./examples/guarded-property-coverage.md|guarded-property-coverage.md]].
- Cecil exposes "what does this method call," never "who calls this method" — a transitive/call-graph check must be written explicitly, with a `visited` guard against cycles; there is no free reverse index to query instead.
- A single-pass check (scan every method once, flag) and a call-graph/registry-driven check are different in kind — keep them in separate test classes, so a broken recursive check's failure list doesn't bury a broken simple one.
- Each of the four checks is paired with a `.feature` file that describes, in plain language, what it proves — but the file is documentary, not executable Gherkin: the actual proof is the `[Fact]` itself, since a structural fact ("is this dead," "is this thrown from the right place") has no natural Given/input → Then/output shape a Reqnroll step could meaningfully parameterize. Scenario titles mirror the `[Fact]` method name 1:1 so a reader can find the corresponding test without a real step-definition binding to follow. See [[skills/dotnet/architecture/draft/solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/{Check}.feature.create|{Check}.feature]]

# Boundaries
- This documentary-`.feature` allowance is scoped to structural/architecture facts specifically — it is not a general license to skip step-definition bindings elsewhere in the codebase; every other `.feature` file in this catalog (including `{Module}.Domain.Rules.Spec`'s own rule-correctness scenarios) still needs a real, executing binding per [[skills/dotnet/architecture/draft/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]'s own MUST rule
- The four checks here are fixed and specific (dead-rule, exception-scoping, code-uniqueness, guarded-property-coverage) — this solution does not define a general framework for writing arbitrary new Cecil checks; a genuinely new kind of structural fact is a new worked example added to this solution, not a plug-in point it exposes
- `GuardedPropertyRuleCoverageTests` only scans `{Module}.Domain` — it does not scan `{Module}.Domain.Rules`, `{Module}.Application`, or any other module's assembly; narrowing a guarded setter to `private` is what actually closes an external bypass, this test only catches what's left reachable within `Domain` itself (see [[./examples/guarded-property-coverage.md|guarded-property-coverage.md]])
- This solution proves the rule *mechanism* is wired correctly as a whole — it does not prove any individual rule's own condition is correct; that is `{Module}.Domain.Rules.Tests`'s job, via [[skills/dotnet/architecture/draft/solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]

# Adr

- [[./adr/cecil-over-reflection.md|Mono.Cecil over Reflection/Roslyn for architecture tests]]
  - Selected variant: Mono.Cecil, as a plain xUnit `[Fact]` alongside the rest of the module's conformance suite.
- [[./adr/registry-driven-coverage-over-per-rule-tests.md|Registry-driven coverage check over one test per rule]]
  - Selected variant: one generic registry-driven Cecil test, paired with narrowing guarded setters to `private` wherever the write pattern allows it.

# Requirements

SOLUTION:
- [[skills/dotnet/architecture/draft/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill.md|solution-dotnet-conformance-testing]]
  - [[skills/dotnet/architecture/draft/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Domain.Tests.csproj.create.md|{Module}.Domain.Tests.csproj]] — hosts the `Architecture/` folder these tests live in, already wired into the module's `dotnet test`/`make unit-test` run
- [[skills/dotnet/architecture/draft/solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]
  - [[skills/dotnet/architecture/draft/solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create.md|{Module}.Domain.Rules.csproj]] — the assembly the dead-rule/code-uniqueness checks load; without a real `Domain.Rules` project there is nothing for these two checks to scan

NUGET:
- Mono.Cecil {existing solution version} — `AssemblyDefinition`, `TypeDefinition`, `MethodDefinition`, `Instruction`, `MethodReference`/`FieldReference` — the entire object model these tests are built on.

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/draft/solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend|{Module}.Domain.Tests.csproj]] - extend - Add the `Architecture/` folder holding all four checks
  - [[skills/dotnet/architecture/draft/solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/{Module}ArchitectureTests.cs.create|{Module}ArchitectureTests.cs]] - create - The three single-pass checks (dead-rule detection, exception scoping, code-uniqueness/format), one class, three `[Fact]`s — see [[./examples/dead-rule-detection.md|dead-rule-detection.md]], [[./examples/exception-scoping.md|exception-scoping.md]], [[./examples/code-uniqueness-format.md|code-uniqueness-format.md]] for the full worked implementation and rationale
  - [[skills/dotnet/architecture/draft/solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/GuardedPropertyRuleCoverageTests.cs.create|GuardedPropertyRuleCoverageTests.cs]] - create - The registry-driven call-graph check, its own class — see [[./examples/guarded-property-coverage.md|guarded-property-coverage.md]] for the full worked implementation and rationale
  - [[skills/dotnet/architecture/draft/solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/{Check}.feature.create|{Check}.feature]] - create - Documentary `.feature` file per check, scenario titles mirroring `[Fact]` method names

# Workflow

## Add a new dead-code / wiring check

1. Identify the "meaningful entry point" production code is supposed to call (e.g. `Check()`, not the raw predicate it wraps).
2. Load the assembly(ies) that declare the entry point and the assembly that should call it.
3. Collect all `Code.Call`/`Code.Callvirt` targets across every method with a body; compare `MethodReference.FullName` against the entry-point list.
4. Report any entry point never found among the collected calls.

## Add a new exception/type-scoping check

1. Decide the allowed namespace(s)/type(s) for the origin of the exception.
2. Walk every method body outside the allowed namespaces for `Code.Newobj` whose `DeclaringType.FullName` matches the exception type.
3. Report every match found outside the allowed namespaces.

## Add a new generated-constant check

1. Identify the field naming convention (e.g. `...Code`) and the expected value format (a `Regex`).
2. Collect `f.HasConstant` fields matching the naming convention across the target namespace.
3. Assert format compliance and uniqueness over the collected values.

## Add coverage for a new multi-field rule

1. Confirm [[./examples/guarded-property-coverage.md|the existing generic test]] already covers the target Entity's namespace (it does, if the Entity lives under the module's `Domain/Entities` namespace).
2. Add one `(nameof(Entity), nameof(Entity.Property))` → `["RuleClass.Check"]` line to the registry, one line per guarded property.
3. Where the write pattern allows it, narrow the guarded property's own setter from `internal`/`public` to `private` — this closes external bypasses at compile time, on top of what the test catches. See [[./examples/guarded-property-coverage.md|guarded-property-coverage.md]] for why this is necessary, not optional, for a setter the test would otherwise flag on its own.

# Rules

## MUST
- Load every assembly under test via `typeof(KnownType).Assembly.Location` — never a hardcoded or environment-relative path.
  - Risk: a hardcoded path breaks the moment the build output layout changes (Debug/Release, target framework folder, CI vs local), and fails silently as "assembly not found" rather than as a meaningful architecture violation.
  - Fix: always resolve the path from a `typeof(...)` already known to live in the target assembly.
- Keep a coverage registry (e.g. `RequiredRuleChecks`) inside the test project, never in `Domain`/`Domain.Rules` production code.
  - Violation: moving the `(Entity, Property) -> Rule` dictionary into `Domain` so "production code documents its own invariants."
  - Risk: `Domain.Rules` referencing `Domain`'s Entity types (or vice versa, depending on which side holds the registry) creates a dependency a portable, other-service-consumable Rules project must not carry — see [[./adr/registry-driven-coverage-over-per-rule-tests.md]].
  - Fix: the registry is verification metadata; it lives next to the test that reads it.
- Guard every recursive call-graph walk with a `visited` set keyed by `MethodDefinition`.
  - Risk: two Entity methods (or private helpers) calling each other, directly or indirectly, causes unbounded recursion and a stack overflow instead of a clean test failure.
  - Fix: `if (!visited.Add(method)) return;` at the top of the recursive function, before touching its instructions.
- Never rely on a bespoke, per-rule Cecil test as the only guard for "does every Entity method call this rule."
  - Violation: writing a new `XyzRule_IsCalledEverywhere` test class for every multi-field rule instead of adding a line to the shared registry.
  - Risk: N rules become N near-identical tests to remember to write; forgetting to write test N+1 is exactly the same failure mode ("agent forgot") the mechanism exists to prevent, just moved one level up.
  - Fix: one generic, registry-driven test (see [[./examples/guarded-property-coverage.md]]); a new rule adds a dictionary entry, not a new test class.
- Never put a multi-field rule's check logic inside an individual property setter.
  - Violation: `internal set { Check(value, this.OtherProperty); field = value; }` for a rule spanning two properties.
  - Risk: object-initializer/constructor property assignment happens in a fixed order — a sibling property may still hold its default or previous value when this setter's check runs, so the check validates a transient combination, not the final one. It can silently accept an invalid final state or reject a valid one, depending purely on assignment order.
  - Fix: keep the rule call in a constructor or a dedicated method that receives every coupled value atomically in one call (e.g. `Entity.Create(...)`), and narrow the individual setters to `private`.
- Give each check a documentary `.feature` file with no real step-definition binding, not a fabricated one that pretends to bind Given/When/Then to production code.
  - Violation: writing hollow `[Given]`/`[When]`/`[Then]` step methods that exist only to satisfy `solution-dotnet-conformance-testing`'s "every scenario has a binding" rule, without the step actually calling anything meaningful.
  - Risk: a fake binding looks like real BDD coverage in a report but proves nothing beyond "the string matched a regex" — worse than no `.feature` file at all, since it actively misrepresents what was verified.
  - Fix: the `.feature` file documents intent only; the `[Fact]` next to it is the actual, sole proof. Say so explicitly in the file (see [[skills/dotnet/architecture/draft/solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/{Check}.feature.create|{Check}.feature]]).

## SHOULD
- Prefer narrowing a guarded property to `private` over trying to catch external bypasses by loading and scanning every assembly with `InternalsVisibleTo` access to it.
  - Risk: `internal` visibility gives a bounded but ever-growing list of assemblies to keep loading and scanning as the solution grows, and `public` visibility gives no bounded list at all — no Cecil-based scan can ever be complete for a `public` member. See [[./adr/registry-driven-coverage-over-per-rule-tests.md]].
  - Fix: `private set` makes the compiler reject the bypass at every caller's compile time, permanently, at zero ongoing maintenance cost — strictly stronger than any scan, for the callers a scan could even reach.
- Keep a call-graph/registry-driven check in its own test class, separate from single-pass checks in the same module.
  - Risk: mixing a complex recursive check with three simple single-pass ones in one file makes the file's own complexity budget harder to reason about, and a failure in the complex check's output is easy to skim past among simpler ones.
  - Fix: one class per check that needs its own registry or its own recursive traversal (see `GuardedPropertyRuleCoverageTests`); simple single-pass checks may continue to share one class.

## MAY
- Reuse the same `LoadDomainAssembly()`/`LoadDomainRulesAssembly()`-style helper method verbatim across test classes in the same project — this is intentional, low-cost duplication (one `AssemblyDefinition.ReadAssembly` line), not a reason to extract a shared base class.

# Check list

- [ ] Every architecture test loads its target assembly via `typeof(KnownType).Assembly.Location`.
- [ ] Any coverage registry lives in the test project, not in `Domain`/`Domain.Rules` production code.
- [ ] Every recursive call-graph walk carries a `visited` guard.
- [ ] A new multi-field rule adds a registry entry, not a new bespoke test class.
- [ ] No rule-check logic lives inside an individual property setter for a rule spanning more than one property.
- [ ] Call-graph/registry-driven checks live in their own test class, separate from single-pass checks.
- [ ] A guarded property's setter is narrowed to `private` wherever the write pattern allows it, not left `internal`/`public` "because the test will catch misuse anyway."
- [ ] Each of the four checks has a companion documentary `.feature` file, scenario titles matching `[Fact]` method names, with no fabricated step-definition binding
- [ ] `GuardedPropertyRuleCoverageTests` scans `{Module}.Domain` only — no other module's assembly
