---
name: reads-require-persistence
description: What the REST transport requires from the rest of the catalog in v3.1
problem: v3 gated solution-http-api-publication on "at least one of command-integration or query-integration". v3.1 makes command/query dispatch common and the repository-backed read side part of persistence (VP2) — the v3 constraint no longer describes anything real.
decision: solution-http-api-publication depends_on solution-api-project + solution-mediator-integration only. POST/PUT/PATCH/DELETE are always available (dispatch is common); GET actions are added only when solution-query-integration (VP2) is present. A module without persistence gets a write-only REST surface.
tags:
  - solution/http-api-publication
  - stack/dotnet
  - concern/documentation
  - concern/documentation/adr
---

# Problem

v3's `solution-http-api-publication` `depends_on solution-query-integration` + `solution-command-integration`, backed by an ADR requiring "at least one MediatR handler source". In v3.1:

- The MediatR dispatch mechanism (`ICommand`, `IQuery`, handler conventions) is **common** (`solution-mediator-integration`) — every module has it.
- The **repository-backed** query handler and `App.Queries` read models are part of **Persistence (VP2)** (`solution-query-integration`).

So "requires command or query integration" is always trivially true for the command half, and the query half is really "requires VP2". The constraint as written describes nothing.

# Selected variant

**Selected variant:** [[#depends_on api-project + mediator-integration; GET actions gated on VP2]]

# Searched variants

## Keep v3's "requires command or query integration" constraint

### Description
Port the v3 ADR and `depends_on` unchanged.

### Benefits
- No change.

### Costs
- The command half is always satisfied (dispatch is common) — the constraint never fires.
- The query half points at a solution that is now inside persistence — the dependency is really on VP2, mislabelled.
- A reader checking the constraint learns nothing.

## depends_on api-project + mediator-integration; GET actions gated on VP2 (selected)

### Description
`depends_on solution-api-project` (the `{Module}.Api` project + `ApiRegistration`) and `solution-mediator-integration` (the markers). Controllers' write actions (POST/PUT/PATCH/DELETE) are always emitted. GET actions across the five controller types are emitted only when `solution-query-integration` (VP2) is also applied — otherwise the module has a **write-only** REST API, which is a complete, valid application of this solution.

### Benefits
- The dependency graph is truthful: REST needs the API project and the dispatch markers, nothing else.
- "write-only API without persistence" is stated as a first-class outcome, not an edge case.
- The Variability Map's VP8 row carries no constraint, matching reality.

### Costs
- The five-controller-type templates need a note per action distinguishing "always" from "GET, needs VP2" — already implicit in v3's Boundaries, now explicit.
