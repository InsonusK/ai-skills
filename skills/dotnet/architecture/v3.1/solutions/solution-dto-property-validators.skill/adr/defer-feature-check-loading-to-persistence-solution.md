---
name: defer-feature-check-loading-to-persistence-solution
description: Why {Feature}Check.cs.create.md leaves Load unimplemented instead of showing a concrete IReadRepository<T> example, and why the concrete realization is deferred to solution-repository-integration's own .extend.md on the same class.
problem: {Feature}Check's worked example previously injected IReadRepository<T>, a type that does not exist at this solution's built_on_plateau (plateau-stateless-non-interactive-service). The example was illustrative only, but nothing distinguished it from a real, applicable pattern, and every plateau that composed this solution without persistence (plateau-service-with-validated-module-interaction, plateau-shared-rules) copied the same forward reference into its own structural skill.
decision: Leave Load unimplemented (throwing) in solution-dto-property-validators's own {Feature}Check.cs.create.md. Let solution-repository-integration — the solution that actually introduces IReadRepository<T> — supply the concrete Load body via its own {Feature}Check.cs.extend.md, targeting the same class, merged in by plateau-create-by-solutions/plateau-update-by-solutions wherever both solutions are composed together (starting at plateau-statefull-service).
tags:
  - solution/dto-property-validators
  - concern/documentation
  - concern/documentation/adr
  - stack/dotnet
---

# Problem

`{Feature}Check` is a DI-injected async wrapper that preloads data for a cross-aggregate validator condition. Its worked example needs *some* data-loading call to be concrete and useful — but `solution-dto-property-validators` is `built_on_plateau: plateau-stateless-non-interactive-service`, which has no repository or any other data-loading abstraction at all.

The previous version of `{Feature}Check.cs.create.md` injected `IReadRepository<T>` (from `solution-repository-integration`) directly in its worked example, with a prose caveat that the type "does not exist until `solution-repository-integration` is composed." This produced two problems:

- Every plateau that composed `solution-dto-property-validators` without persistence (`plateau-service-with-validated-module-interaction`, and transitively `plateau-shared-rules`) copied this same forward reference — a class skill referencing a type that plateau's own lineage does not provide.
- The caveat was prose only. Nothing in the class's actual shape signaled "not real yet" — an agent applying the solution to a genuinely stateless module could copy the worked example verbatim and produce code that does not compile.

# Selected variant

**Selected variant:** [[#Leave Load unimplemented; defer the concrete body to solution-repository-integration]]

`{Feature}Check.cs.create.md` now shows `Load` throwing `NotSupportedException`, with a comment pointing at whichever solution introduces a data-loading abstraction. `solution-repository-integration` — which already `extends` `{Module}.Application.csproj` for its own specs — adds a new `{Feature}Check.cs.extend.md`, in the same `Before`/`After` style `solution-domain-rules` already uses for this same class, replacing the stub `Load` with a concrete `IReadRepository<T>`-based implementation. `CheckAsync` is untouched by that extension.

- No plateau ever contains a `{Feature}Check` referencing a type its own lineage doesn't provide — the abstract form is always compilable-in-spirit (a deliberate `NotSupportedException`, not a dangling reference).
- The concrete realization is attributed to the solution that actually owns the capability (`solution-repository-integration`), visible in that plateau's `__Applied solutions:__` trailers, instead of being silently baked into `solution-dto-property-validators`'s own worked example.
- No new field, no new solution, no new "Plateau Component" is needed — this reuses the same cross-solution `.extend.md` mechanism `solution-domain-rules` already established for this exact class.

# Searched variants

## Keep the concrete IReadRepository<T> example with a prose caveat (status quo)

### Description
Keep injecting `IReadRepository<T>` directly in `{Feature}Check.cs.create.md`'s worked example, relying on the existing "worked example is illustrative until composed" sentence in `# Boundaries`.

### Costs
- The forward reference reappears in every plateau that composes this solution without persistence — a structural skill file describing a type its own plateau lineage never provides.
- Nothing about the class's own shape marks it as non-functional; an agent can copy it as-is into a genuinely stateless module.

## Extract a "solution-shared-repository-project" contract solution, applied early

### Description
Split `IReadRepository<T>`'s interface declaration into its own solution, composed into the base plateau (`plateau-stateless-non-interactive-service`) ahead of persistence, so `{Feature}Check` always has something real to inject.

### Costs
- Adds a repository interface — a capability the base plateau explicitly does not have — to the one plateau whose entire identity is "no repository, no data-loading abstraction, no external interaction." This dilutes that plateau's own definition for the sake of one class's worked example.
- Introduces a third category of unit (a "contract solution") alongside Solution/Plateau/Plateau Component, solving one instance of the problem instead of reusing the cross-solution `.extend.md` mechanism the codebase already has for exactly this class (`solution-domain-rules`'s own `{Feature}Check.cs.extend.md`).

## Build a dedicated bridging solution that wires solution-repository-integration into {Feature}Check

### Description
Create a new, separate solution whose only job is to inject `IReadRepository<T>` into `{Feature}Check`, `depends_on` both `solution-dto-property-validators` and `solution-repository-integration`.

### Costs
- Adds a solution, and a `depends_on` edge, purely to move code that already has a natural, established home (`solution-repository-integration`'s own `Implementation/`, via `.extend.md`) — more indirection for no additional capability, and previously rejected for the same reason in this codebase's own design discussion.
