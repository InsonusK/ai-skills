---
name: format-semantic-domain-unification
description: Whether Format, Semantic, and Domain validation are three distinct mechanisms or one mechanism applied to differently-sourced input.
problem: Format (one field), Semantic (several fields of one DTO/Entity), and Domain (state across several Entities) validation were treated as three separate shapes with their own examples and conventions, risking three things to learn and keep consistent instead of one.
decision: All three are the same mechanism — IsValid()/IRuleBuilder-extension/Check() over a wrapper of the fields the rule needs — differing only in where that wrapper comes from.
tags:
  - solution/domain-rules
  - concern/documentation
  - concern/documentation/adr
  - stack/dotnet
---

# Problem

Earlier documentation for this solution treated Format, Semantic, and Domain validation as three separate mechanisms, each demonstrated with its own worked example and its own implicit conventions. In practice every example converged on the identical shape: a static class with `IsValid()`, an `IRuleBuilder<T, TWrapper>` extension, and a `Check()` convenience — the only thing that varied between examples was **where `TWrapper`'s value came from**, not how the rule itself was written or wired. Left implicit, this either gets rediscovered per rule (wasted effort) or drifts into three genuinely different conventions (maintenance cost, harder onboarding).

# Selected variant

**Selected variant:** [[#One mechanism, classified by wrapper origin]]

# Searched variants

## One mechanism, classified by wrapper origin (selected)

### Description

A rule is always: bundle the values it needs into a wrapper (an existing `Soft{ValueObject}` property, a `Soft{ValueObject}` assembled ad hoc, or an anonymous tuple), then apply `IsValid()`/`IRuleBuilder`-extension/`Check()` to that wrapper — identical in every case. What differs is only where the wrapper's values come from:

| | Format | Semantic | Domain |
|---|---|---|---|
| Wrapper source | Already a property of the container (`TodoTask.Complexity : SoftComplexity`) | Assembled on the spot from the container's **own** other fields (`new SoftSchedule(dto.StartDateTime, dto.DueDateTime)`) | Assembled on the spot from fields **loaded** from elsewhere (`(transaction.Account.Balance, delta)` after a repository call) |
| Needs I/O to assemble? | No | No | Yes — but the loading is the caller's job, never the rule's |
| Wrapper shape | Usually a named `Soft{ValueObject}` | Named `Soft{ValueObject}` (reusable concept) or anonymous tuple (rule-local) | Same choice as Semantic |

![wrapper-mechanism](./diagrams/wrapper-mechanism.mmd)

Three arrows into the wrapper `W` at the top, one shared pipeline below it. Format's arrow is the container handing over a value it already has; Semantic's arrow assembles the wrapper from the container's own other fields, no I/O; Domain's arrow does the same assembly, but only after a `Load` step brings in values from somewhere else. Everything below `W` — `IsValid()`, the `IRuleBuilder` extension, `Check()`, and the two consumers (fail-fast VO/Entity, collect-all DtoValidator) — is identical regardless of which arrow was taken. Domain validation is, mechanically, Semantic validation preceded by a loading step: "Domain validation = data preload + semantic validation." The rule itself never knows or cares which arrow it came from.

The choice between a **named `Soft{ValueObject}`** and an **anonymous tuple** for the wrapper is orthogonal to Format/Semantic/Domain — it is a separate decision per rule: name the wrapper when the combination of fields is itself a reusable domain concept (`Schedule` — a task's time window, potentially a real Entity property on its own), leave it an anonymous tuple when the combination has no meaning beyond this one comparison (`(Balance, Amount)`, `(ParentId, ChildId)`). A named wrapper additionally justifies a `PropertyValidator`-equivalent DI/isolated-testing layer; a rule-local tuple usually does not need one.

### Benefits

- One shape to teach, review, and mutation-test, not three — a new rule author copies the same three-member pattern regardless of which kind of validation they are writing.
- Makes the Format/Semantic boundary precise instead of intuitive: checking a `SoftSchedule` value in isolation is Format even though it wraps two dates, because the wrapper already exists as one property; checking two of a DTO's own separate `DateTimeOffset?` fields is Semantic, because the wrapper had to be assembled. Wrapping two fields is not, by itself, what makes something Semantic.
- Domain validation stops looking like a special case requiring its own testing/wiring story — it reuses every adapter already built for Semantic validation, plus one loading step that lives outside `Domain.Rules` entirely.

### Costs

- The Format/Semantic boundary now requires the reader to know whether a wrapper "already exists as a property" or was "assembled on the spot" — a distinction invisible from the rule's own code, only visible from the caller.
- Two wrapper shapes (named `Soft{ValueObject}` vs. anonymous tuple) mean two syntactically different `IRuleBuilder` extension signatures to recognize, even though they play the same role.

## Three separate mechanisms (Format ≠ Semantic ≠ Domain)

### Description

Keep Format, Semantic, and Domain validation as three independently-designed shapes, each with its own example, its own adapter story, and its own conventions for how the rule is declared and wired.

### Benefits

- Each kind of validation can be explained in complete isolation, with no need to first understand the other two.

### Costs

- Three things to keep consistent instead of one; a fix or improvement discovered while working on Format validation has to be manually ported to Semantic and Domain examples separately, and did in fact fall out of sync during this solution's own development.
- New rule authors have no single template to copy — they must first classify their rule as Format/Semantic/Domain and then find the matching, separately-maintained convention.
- Obscures that Domain validation's only genuinely new concern is the loading step (async, repository-bound) — bundled with a full separate "mechanism," that one real difference is harder to isolate and test on its own.

## Keep three kinds in vocabulary only, without unifying the code shape

### Description

Keep the Format/Semantic/Domain terminology for communication (this is how a rule is *classified*), but do not claim or document that the underlying code is the same mechanism — treat the code-level similarity as coincidental rather than load-bearing.

### Benefits

- Avoids committing to "always use `IsValid()`/`IRuleBuilder`-extension/`Check()`" as a hard rule for every future kind of validation that might not fit it.

### Costs

- Leaves the actual, observed pattern undocumented, so it has to be rediscovered by every reader instead of stated once — the exact problem this ADR exists to close.
- Provides no criterion for when a genuinely new kind of validation (not fitting the wrapper pattern) has been found versus when a rule author simply did not recognize the existing pattern applies.
