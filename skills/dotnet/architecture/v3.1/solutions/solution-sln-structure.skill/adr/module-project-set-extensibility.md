---
name: module-project-set-extensibility
description: The guaranteed per-module base set in v3.1 is two projects (Interfaces, Application); Domain and Api are pattern-solution additions gated by their feature.
problem: v3's solution-sln-structure created four projects per module (Api, Application, Domain, Interfaces) unconditionally. The v3.1 Feature Model makes DomainLogic and the inbound-sync API optional, so a module can legitimately have neither a Domain layer nor an Api surface — an unconditional four-project scaffold contradicts that.
decision: Reduce the guaranteed base to Interfaces + Application. {Module}.Domain is created by solution-domain-behaviour, {Module}.Api by solution-api-project, further projects by whichever pattern solution needs the isolation. solution-sln-structure does not enumerate the optional projects — the solution dependency graph does.
tags:
  - solution/sln-structure
  - stack/dotnet
  - concern/documentation
  - concern/documentation/adr
---

# Problem

In v3, `solution-sln-structure` was the foundation solution and created every module with four projects: `Api`, `Application`, `Domain`, `Interfaces`. The [v3.1 Feature Model](skills/dotnet/architecture/v3.1/feature/feature-model.md) and [Variability Map](skills/dotnet/architecture/v3.1/variability-map.md) establish that:

- **DomainLogic** (VP1) is optional — a module can orchestrate other modules and shape data with no entities and no `{Module}.Domain` project at all. The Feature Model's baseline explicitly has no Domain project.
- The **inbound sync API** (VP8/VP9) is optional — a module reachable only over async messaging, or only as an internal MediatR target, has no `{Module}.Api` project.

An unconditional four-project scaffold forces every module to carry an empty `Domain` and an empty `Api`, which invites anemic entities, hides whether a feature is actually present, and contradicts the Feature Model's own baseline.

# Selected variant

**Selected variant:** [[#Base set of two (Interfaces, Application), Domain and Api pattern-added]]

# Searched variants

## Keep the v3 four-project base

### Description
`solution-sln-structure` keeps creating `Api`, `Application`, `Domain`, `Interfaces` for every module; `solution-domain-behaviour` and the API solutions only `extend` them.

### Benefits
- No change from v3; every existing v3 solution's `extends` list still resolves.
- A module's project list is fixed and predictable.

### Costs
- Directly contradicts the v3.1 Feature Model baseline ("no `{Module}.Domain` at this baseline").
- An empty `Domain` project is a standing invitation to put a bare entity there without `solution-domain-behaviour`'s guards.
- "Does this module have a domain layer / an API?" can no longer be answered by looking at its project list.

## Base set of two (Interfaces, Application), Domain and Api pattern-added (selected)

### Description
`solution-sln-structure` guarantees `Interfaces` + `Application` per module. `{Module}.Domain` is created by `solution-domain-behaviour` (VP1). `{Module}.Api` is created by a new shared `solution-api-project` (VP8 + VP9 prerequisite). Further projects (`{Module}.Domain.Rules`, persistence layers) are created by their owning pattern solution. `solution-sln-structure` states the base and the general extension permission; the concrete optional-project list is read from the dependency graph.

### Benefits
- Matches the Feature Model baseline exactly.
- A module's project list reflects its selected features — `Domain` present ⇔ DomainLogic selected.
- Keeps `solution-sln-structure` free of churn from unrelated pattern decisions (same reasoning as v3's original extensibility ADR, extended one level).

### Costs
- Every v3 solution that `extends {Module}.Domain.csproj` / `{Module}.Api.csproj` must now name the creating solution in its `depends_on` (a required part of the v3.1 migration anyway).
- `solution-sln-structure` alone no longer answers "the full project list of a fully-built module" — the reader consults the dependency graph too.

## Base of two, but a single "module projects" solution owns Domain + Api creation

### Description
One `solution-module-projects` creates `Interfaces` + `Application` + (conditionally) `Domain` + `Api`, driven by flags.

### Benefits
- One place to look for module project creation.

### Costs
- A flag-driven solution that conditionally creates projects is exactly the "combination-resolver" smell the Feature Model calls out for entity classification — it couples independent features into one solution.
- `solution-domain-behaviour` already owns everything else about the domain layer; splitting "create the project" from "define its contents" adds a seam for no benefit.
