---
name: shared-rules
description: Composes the statefull-service plateau with a centralized, portable rule mechanism ({Module}.Domain.Rules) and its build-time correctness guarantees — a condition duplicated across a VO, an Entity, a PropertyValidator, and a DTO/Command validator gets exactly one declaration, proven from every layer that redirects to it via one shared Gherkin source (including the now-real, persisted `{Feature}Check` path), plus four Mono.Cecil structural checks that no BDD scenario can give by construction.
whenToUse: when the same validation condition has been duplicated across two or more of a module's VO/Entity/PropertyValidator/DtoValidator and needs one shared, reusable, cross-adapter home — or when reviewing whether a centralized rule, its `.feature` coverage, or its structural wiring (dead rules, exception scoping, code uniqueness, guarded-property coverage) follow this baseline
domain: skill
type: template
version: 20260824163000
tags:
  - skill/template/plateau
  - plateau/shared-rules
parent_plateaus:
  - "[[skills/dotnet/architecture/draft/plateau/plateau-statefull-service/plateau-statefull-service.skill/plateau-statefull-service.skill.md|plateau-statefull-service]]"
standalone: false
created_by:
  - "[[../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
  - "[[../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]]"
adr:
  - "[[skills/dotnet/architecture/draft/plateau/plateau-shared-rules/adr/rebind-to-statefull-service-parent.md|Rebind to plateau-statefull-service as the only parent]]"
---

# Goal
Give a module a single, reusable home for a business predicate once it turns out to be duplicated across the validation layer's own consumers, plus a build-time guarantee that the redirection actually holds everywhere it claims to:
- Create `{Module}.Domain.Rules` — a dedicated, portable project holding every centralized predicate (`{Rule}.cs`), reusable by another .NET service without adopting this service's exception/pipeline conventions
- Redirect an already-working local condition in `{ValueObject}.cs`, `{EntityName}.cs`, `{ValueObject}PropertyValidator.cs`, or `{Dto}.Validator.cs` to call the centralized rule instead — never speculatively, only once genuine duplication is observed
- Give every centralized rule one shared Gherkin source (`{Module}.Domain.Rules.Spec`), proven independently from the rule itself, its VO/Entity fail-fast adapter, and its DtoValidator collect-all adapter — without three copies of the same scenario text
- Give the rule mechanism as a whole four Mono.Cecil structural guarantees over compiled IL: no rule is dead code, `DomainException` is thrown only from the right layer, every generated rejection code is unique and well-formed, and every Entity write of a rule-guarded property actually calls that rule

# Core Principles
- Inherited from [[skills/dotnet/architecture/draft/plateau/plateau-statefull-service/plateau-statefull-service.skill/plateau-statefull-service.skill.md|plateau-statefull-service]] (and, transitively, everything it composes): fixed four-project module shape, centralized pipeline/module registration, global unhandled-exception handling, the `make unit-test`/`mutation-test`/`test-report`/`test-and-report` conformance gate, Value Objects at both strengths, guarded entity behavior, the `ValidationBehavior` pipeline gate, cross-module DTO/VO validators (`{ValueObject}PropertyValidator`/`{Dto}Validator`/`{Feature}Check`), the full `ICommand`/handler/validator/registration chain, and real persistence (`AppDbContext`, `Repository<T>`, `IUnitOfWork`, concurrency/idempotency/timestamps, `App.Queries`) — this plateau does not change any of that, it gives a proven-duplicated piece of it one shared home.
- This plateau's parent is `plateau-statefull-service`, not `plateau-service-with-validated-module-interaction` — see [[../adr/rebind-to-statefull-service-parent.md|this plateau's own ADR]]. `{Feature}Check`'s `Load` step is therefore real here (inherited concretely, via `IReadRepository<T>`), not the throwing stub earlier plateaus define — so a Domain-classified rule's `.Check()` redirect is provable end to end, through a real loaded value, not just illustratively. Rule-centralization itself remains orthogonal to persistence in principle; this plateau simply always has both, by construction of where it sits in the lineage.
- Optional and never speculative: applying this plateau's centralization to a module is a refactor of existing, already-working code, triggered only by real, observed duplication — `solution-value-objects`, `solution-dto-property-validators`, and `solution-domain-behaviour` each already work completely standalone, with nothing about them requiring `{Module}.Domain.Rules` to exist.
- One rule shape everywhere: bundle the values a rule needs into a wrapper, then apply `IsValid()`/`IRuleBuilder`-extension/`Check()` to it. Format (wrapper already a container property), Semantic (wrapper assembled from the container's own fields), and Domain (wrapper assembled from data loaded elsewhere) differ only in where the wrapper's values come from — never in how the rule is written or wired. `ErrorCode`/default `Message`/`State` are declared exactly once, inside the `IRuleBuilder` extension; every other adapter forwards its `ValidationResult`, never re-declares. See [[../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]].
- A same-aggregate Domain rule stays synchronous; a cross-aggregate/cross-service Domain rule becomes Try/Confirm — the saga's own Handler/Consumer wiring is not created by this plateau, only illustrated to show when it's needed.
- `EntityNotLoadedException` (Shared) is a Handler defect — a required navigation was not preloaded — never confused with `DomainException`; it maps to `500` + critical log, never the same path as invalid input.
- One shared Gherkin source per rule, three independent proofs: `{Module}.Domain.Rules.Spec/{Rule}.feature` is linked into `{Module}.Domain.Rules.Tests` (all scenarios, proving the rule itself), `{Module}.Domain.Tests` (`@format`-tagged only, proving the VO/Entity fail-fast adapter), and `{Module}.Application.Tests` (`@semantic`/`@domain`-tagged only, proving the DtoValidator/`{Feature}Check` collect-all adapter). `{Module}.Domain.Rules.Spec` is a directory, not a project — it produces no assembly and is referenced by nothing; every consumer links the physical `.feature` file in via its own `.csproj`. See [[../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md#prove-a-rule-from-every-layer-that-redirects-to-it|solution-domain-rules' Workflow]].
- `{Module}.Domain.Rules.Tests` isolates `{Module}.Domain.Rules`'s own mutation-testing surface from `{Module}.Domain.Tests`'s broader one, mirroring the same one-test-project-per-production-project pattern the base plateau already establishes for the other five projects.
- Structural guarantees, not behavioral ones: the four Mono.Cecil checks in `{Module}.Domain.Tests/Architecture` read compiled IL rather than execute it — dead-rule detection (every `Check()` is called by production code outside `Domain.Rules`), exception-type scoping (`DomainException` constructed only from `ValueObjects`/`Entities`), generated-constant uniqueness/format (every rejection code unique, `{ModuleName}.{Class}.{Reason}`), and registry-driven guarded-property coverage (every Entity write of a rule-guarded property calls that rule, via a recursive call-graph walk with a `visited` guard). A coverage registry lives in the test project, never in production code. See [[../../../solutions/solution-cecil-architecture-tests.skill/solution-cecil-architecture-tests.skill.md|solution-cecil-architecture-tests]].
- Each Cecil check is paired with a documentary `.feature` file — no real step-definition binding, since a structural fact has no natural Given/input → Then/output shape; the `[Fact]` next to it, with a matching scenario title, is the actual proof. This is a scoped, explicit exception to the base plateau's "every scenario has a matching binding" rule, not a general license.
- Not standalone: `standalone: false` — still no HTTP API surface. A module can apply this plateau's centralization and structural guarantees on its own, on top of the persistence it now always inherits.

# Capabilities
- domain (inherited)
  - Value Objects at both strengths, entities with guarded behavior, static domain services for bulky logic.
- validation (inherited)
  - `ValidationBehavior` before every handler; cross-module `{ValueObject}PropertyValidator`/`{Dto}Validator`/`{Feature}Check`, resolvable via `IValidator<T>`.
- commands (inherited)
  - The full `ICommand<TResponse>` → handler/validator → module self-registration chain.
- persistence (inherited)
  - `AppDbContext`, generic `Repository<T>`, `IUnitOfWork`, concurrency/idempotent-creation/timestamps, `App.Queries` cross-module reads — see [[skills/dotnet/architecture/draft/plateau/plateau-statefull-service/plateau-statefull-service.skill/plateau-statefull-service.skill.md|plateau-statefull-service]]. This is what makes `{Feature}Check`'s `Load` real here.
- rule-centralization
  - One declaration per business predicate, reusable fail-fast (VO/Entity constructor) and collect-all (DTO/Command validator) without divergent logic paths, and reusable unmodified by another .NET service.
  - Decentralized rejection codes (`{ModuleName}.{Class}.{Reason}`) declared next to the rule that produces them.
  - A documented boundary for when a Domain rule can stay synchronous versus when it must become Try/Confirm.
- rule-conformance
  - One Gherkin source per rule, proven independently at up to three layers, with zero duplicated scenario text.
  - `{Module}.Domain.Rules`'s own mutation-testing surface isolated from the rest of `{Module}.Domain`.
- rule-structural-integrity
  - Dead-rule detection, exception-type scoping, rejection-code uniqueness/format, and guarded-property rule coverage — four build-time facts no BDD scenario proves by construction.
  - A new guarded property adds one registry line, never a new bespoke Cecil test class.

# Usecases

## Centralize a condition duplicated across two or more consumers
1. Notice the same condition written independently in two or more of `{ValueObject}.cs`, `{EntityName}.cs`, `{ValueObject}PropertyValidator.cs`, `{Dto}.Validator.cs`.
2. Write `IsValid()` + the `IRuleBuilder` extension (`ErrorCode`/`Message`/`State`) + `Check()` once, in `{Module}.Domain.Rules/{Rule}.cs`.
3. Redirect every duplicate to the new centralized `Check()`/extension — delete the local copies, don't leave both.
4. Write `{Module}.Domain.Rules.Spec/{Rule}.feature`, tagged `@format`/`@semantic`/`@domain` per scenario; add step definitions in `{Module}.Domain.Rules.Tests` (always) and in whichever of `{Module}.Domain.Tests`/`{Module}.Application.Tests` the tags route to.
5. Every consumer keeps behaving exactly as before — only where the condition is declared, and how many places prove it, changed.

## Add a Domain rule that needs data from another aggregate or service
1. Confirm the two Entities are one aggregate (same `Version`/write-lock). If yes, treat it as Semantic — the wrapper is assembled from a preloaded navigation instead of the container's own fields, and stays synchronous.
2. If not — different aggregates or different services — do not attempt ad hoc synchronization: Try (create the dependent Entity `Pending`, using a preliminary check against a possibly-stale local replica) then Confirm (the owning aggregate's existing, unmodified rule/method runs authoritatively, publishing `Confirmed`/`Rejected`).
3. The rule itself does not change between the same-aggregate and Try/Confirm cases — only how and when it gets called does. The saga's Handler/Consumer wiring follows the existing `solution-command-integration` pattern, not created by this plateau.

## Verify the rule mechanism is structurally sound
1. `make unit-test` runs `{Module}ArchitectureTests`/`GuardedPropertyRuleCoverageTests` alongside every other test project, as plain `[Fact]`s.
2. A surviving dead rule, a misplaced `DomainException`, a malformed/duplicate rejection code, or an unguarded property write fails the build — the same gate every other conformance check goes through, per the inherited `make unit-test`/`mutation-test`/`test-report`/`test-and-report` contract.
3. Adding coverage for a new multi-field rule is one registry line in `GuardedPropertyRuleCoverageTests`, never a new bespoke test class.
