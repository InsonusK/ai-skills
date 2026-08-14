---
name: soft-vo-error-accumulation
description: Where value-object validation rules live and how Soft{ValueObject}, the domain Value Object, and FluentValidation validators share them without duplication
problem: Validation rules for shared value objects are duplicated between Domain Rules and Application validators, and consuming modules cannot validate a Soft{ValueObject} without re-implementing the rules
decision: Soft{ValueObject} exposes a computed Errors property backed by rules co-located in {Module}.Interfaces; the domain Value Object inherits from it and throws an aggregated DomainException when Errors is not empty; validators become thin adapters over Errors; checks are classified into three levels with fixed homes
tags:
  - stack
  - concern/documentation/adr
---

# Problem

The default plateau splits value validation across three places:

- `Soft{ValueObject}` in `{Module}.Interfaces` — a plain record with no validation, shared with other modules.
- `{ValueObject}` in `{Module}.Domain` — a sealed record that validates invariants in its constructor via Rules and throws `DomainException`.
- FluentValidation validators in `{Module}.Application` — re-express the same checks for commands and DTOs.

Consequences:

- The same rule (e.g. email format) is written twice: once as a Domain Rule, once as a FluentValidation rule, and the two copies can drift out of sync silently.
- A consuming module that references only `{Module}.Interfaces` cannot validate a received `Soft{ValueObject}` at all without duplicating the producer's rules or referencing the producer's Domain.
- Validators accumulate logic that conceptually belongs to the value object itself.

Decide where validation rules live so that a single source of truth serves the domain value object, the soft value object, and the transport validators.

# Selected variant

**Selected variant:** [[#Computed Errors on Soft VO with rules in Interfaces]]

The approach follows the Notification pattern (collect errors instead of throwing) and matches Vogen's `Validate`/`TryFrom` prior art.

Checks are classified by what they depend on, and each level has a fixed home:

1. **Intra-value checks** (format, range, presence — intrinsic to the concept) — rules co-located with `Soft{ValueObject}` in `{Module}.Interfaces`, consumed via the computed `Errors` property.
2. **Cross-field checks** —
   - fields of one multi-property soft VO (e.g. `SoftDatePeriod(Start, End)` with start-before-end): the check lives in that VO's rules;
   - separate properties of one command: the check stays in the feature validator (`.Must(cmd => cmd.Start < cmd.End)`), because it is a rule of the command, not of any single value object.
3. **Context-dependent checks** (existence in DB, uniqueness, current state) — never in a VO or a validator; they stay in the handler guard or a domain service, because value objects must stay pure, DB checks are asynchronous while `Errors` is synchronous, and existence is a point-in-time fact rather than an invariant of the value.

Endpoint-specific constraints (e.g. `MaximumLength(200)` required by one API contract) do NOT move into Soft VO rules; they stay in the feature validator. Only constraints intrinsic to the concept belong to the Soft VO rules.

# Searched variants

## Computed Errors on Soft VO with rules in Interfaces

### Description

`Soft{ValueObject}` gains a computed (expression-bodied, no backing field) `Errors` property returning `IReadOnlyList<ValidationError>`, produced by rule functions declared next to it in `{Module}.Interfaces`. The domain `{ValueObject}` inherits from `Soft{ValueObject}` and throws an aggregated `DomainException` in its constructor when `Errors` is not empty. Property validators become thin adapters that surface `Errors` through FluentValidation (e.g. via a `.HasNoErrors()` extension). Feature validators keep only cross-property command rules and endpoint-specific constraints; context-dependent checks stay in the handler guard.

### Benefits

- Single source of truth for intra-value rules — no duplication between Domain Rules and validators.
- Consuming modules validate the producer's shapes with the producer's own rules while referencing only `{Module}.Interfaces`.
- The strict VO still cannot exist in an invalid state, and its exception now aggregates all violations instead of failing on the first one.
- A computed property has no backing field, so record structural equality is unaffected, `with` clones re-evaluate from current values, and nothing extra leaks into serialization (`[JsonIgnore]`) or EF Core materialization.
- Cross-field rules inside one multi-property VO move from duplicated `.Must()` validators into the same single source.

### Costs

- `{Module}.Interfaces` now contains validation logic for shared shapes; a rule change becomes a cross-module contract change and consumers pick up new validation behavior transitively.
- A caller that never reads `Errors` can silently ignore validation — the throw-on-construction model made that impossible; mitigated by mandatory validators at the boundary and by the domain VO throwing.
- Rules must return structured errors (code + message) instead of `bool`, so the Rules shape changes.
- FluentValidation degrades to an adapter for VO-backed properties; mapping errors to property paths needs a small extension.
- Validation runs synchronously on every `Errors` access — negligible cost, but it forbids async (context-dependent) checks by construction.

## Soft VO stores errors as instance state at construction

### Description

The original proposal: `Soft{ValueObject}` runs rules in its constructor and stores the resulting errors in a field; the domain VO checks the parent's stored errors after `base(...)` and throws.

### Benefits

- Same single-source-of-truth and cross-module validation wins as the selected variant.
- Errors are computed once, not on every access.

### Costs

- A stored error collection participates in record structural equality — two equal values with equal error lists compare unequal (list reference equality), breaking the "structurally equal by default" guarantee unless equality is hand-written.
- `with` clones copy fields directly without re-running the constructor, so a clone can carry stale or inherited errors.
- The error state leaks into serialization and EF Core materialization — the parameterless constructor produces instances with undefined error state.
- All layering costs of the selected variant still apply.

## Keep validation split between Domain Rules and validators (status quo)

### Description

`Soft{ValueObject}` stays a dumb record; Domain Rules validate the strict VO; FluentValidation validators independently re-express transport-level checks.

### Benefits

- `{Module}.Interfaces` stays free of logic — a pure shape contract.
- Defense in depth: transport validation and domain invariants are independent layers, each changing on its own cadence.
- No restructuring of Rules, VOs, or validators.

### Costs

- Every rule is written twice and the copies can drift out of sync silently.
- Consuming modules cannot validate shared shapes without duplicating the producer's rules.
- The duplication grows with every new shared value object.

## Shared static Check functions with a dumb Soft VO

### Description

Co-locate pure `Check(...) -> errors` functions with `Soft{ValueObject}` in `{Module}.Interfaces`, but expose nothing on the record; validators and the domain VO constructor call the static functions explicitly.

### Benefits

- Same single source of truth and cross-module availability as the selected variant.
- The record stays completely passive — no validation surface on the type at all.

### Costs

- Discoverability is worse: nothing on the type tells a consumer that a Check function exists or must be called.
- Every call site (each validator, each boundary) repeats the wiring, and forgetting it silently skips validation — there is no uniform contract to program against.
- The domain VO constructor keeps per-VO "throw on errors" plumbing instead of reading a uniform `Errors` contract from the shape.
