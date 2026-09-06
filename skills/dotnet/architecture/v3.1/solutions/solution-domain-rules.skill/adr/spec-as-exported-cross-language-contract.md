---
name: spec-as-exported-cross-language-contract
description: Whether {Module}.Domain.Rules.Spec is an internal .NET test-sharing device or a language-agnostic contract meant to be copied into a consumer repo (a frontend, a non-.NET service) and re-proven there.
problem: A frontend or another-language service that must enforce the same validation rules as this service re-derives them from prose, an OpenAPI schema, or trial and error — and drifts from the backend the moment a rule changes, with nothing that fails when it does.
decision: {Module}.Domain.Rules.Spec is authored as an exported contract — one .feature file per rule, scenarios in domain language only, rejection codes verbatim — so a consumer copies the files unchanged and binds them to its own step definitions in its own language. This service owns the scenarios; the consumer owns its bindings.
tags:
  - solution/domain-rules
  - concern/documentation
  - concern/documentation/adr
  - stack/dotnet
---

# Problem

`{Module}.Domain.Rules` centralizes a validation condition into one place *inside this .NET service*. But the same conditions frequently have to hold in a **separate codebase** too — most often a frontend doing optimistic client-side validation, sometimes a sibling service written in another language. Today that second implementation is reconstructed from prose docs, an API schema, or the error messages it observes, and there is no artifact that breaks when the backend rule changes and the client's copy does not. The `{Module}.Domain.Rules.Spec` directory already exists to stop three .NET test projects from copy-pasting the same Gherkin. The question is whether it is *only* that, or whether it is deliberately shaped to be an exportable, cross-language contract.

# Selected variant

**Selected variant:** [[#Exported contract, consumer owns its bindings]]

- The `.feature` files are written to be copied verbatim into another repository. Scenario text is domain language with no .NET/C#/FluentValidation vocabulary and no reference to which adapter proves it; rejection-code strings are part of the contract and appear literally.
- This solution authors and proves the scenarios from every .NET layer. It does **not** own the step definitions of an external consumer — that consumer writes and maintains its own bindings, in its own language, against the same scenario text.
- Sequencing/distribution (how the consumer gets the files — git submodule, package, copy-in-CI) is out of scope here; the decision is only that the artifact is fit to be consumed that way.

# Searched variants

## Exported contract, consumer owns its bindings (selected)

### Description
`{Module}.Domain.Rules.Spec` is a first-class deliverable of this solution, not just a test-sharing device. Its `.feature` files are constrained so that they carry no information specific to this service's implementation: only the rule's condition, its inputs in domain terms, its pass/fail outcome, and the exact rejection code. A frontend or another-language service copies the directory in and writes step definitions binding the identical Gherkin to its own validation code. When a backend rule changes, the scenario changes, the consumer re-syncs the file, and its own bound test fails until its implementation matches.

### Benefits
- A single source of truth for a rule that must hold in two codebases, with a test on *each* side bound to the same text — divergence surfaces as a failing test, not as a production bug.
- The consumer's re-implementation starts from proven scenarios instead of prose, so it is far less likely to miss an edge case the backend already handles.
- Costs almost nothing extra: the same file already had to exist to stop the three .NET test projects duplicating scenario text; the only added constraint is keeping its language implementation-neutral.
- The contract is executable on both sides, unlike an OpenAPI `pattern` or a prose rules doc.

### Costs
- Scenario text must stay disciplined — no `SoftComplexity`, no `ValidationResult`, no "the property validator runs" — which is a rule reviewers have to enforce.
- Rejection codes become a published contract: renaming one is now a breaking change for every consumer, not a local refactor.
- This solution cannot guarantee a consumer actually re-syncs; it only makes drift *detectable* on the consumer side if they wire the scenarios in.
- Nothing here specifies distribution, so each consuming team still has to decide how the files travel.

## Internal test-sharing device only

### Description
`{Module}.Domain.Rules.Spec` exists solely so `{Module}.Domain.Rules.Tests`, `{Module}.Domain.Tests`, and `{Module}.Application.Tests` do not each keep their own copy of a rule's scenarios. Its language is whatever is convenient for those .NET step definitions; it is not intended to leave the repository.

### Benefits
- No extra discipline on scenario wording — steps can say "the property validator runs on X" if that reads well for the .NET bindings.
- Rejection codes stay an internal detail, renamable at will.

### Costs
- The recurring real need — a frontend enforcing the same rules — gets no help; that implementation is still reconstructed from prose and drifts silently.
- The `.feature` files end up coupled to .NET adapter vocabulary, so making them portable *later* is a rewrite of every scenario rather than a constraint honoured from the start.
- Misses that the file is already 90% of a cross-language contract; the only thing standing between "internal" and "exportable" is word choice.

## Publish a machine-readable rule schema instead of Gherkin

### Description
Rather than share `.feature` files, emit each rule as structured data (JSON: input fields, operator, threshold, rejection code) that any consumer parses and evaluates with a generic engine.

### Benefits
- No step definitions to write on the consumer side — one interpreter covers every rule.
- Unambiguous, diffable, versionable.

### Costs
- Only expresses rules that fit the schema's operator set; a rule with any real logic (cross-field, conditional, lookup-shaped) does not serialize and needs an escape hatch, so the schema never covers the whole rule set.
- The consumer runs an interpreter, not its own idiomatic validation code — worse UX integration than native checks on the client.
- Nothing executable ties the backend's own implementation to the schema, so the backend can drift from its own published contract.
- Large new mechanism (schema, generator, interpreter per language) versus reusing Gherkin that already exists here.
