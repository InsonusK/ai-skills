---
name: mediator-pattern-is-one-common-solution
description: Whether the MediatR pattern is one common solution or several per-request-kind solutions, and whether it depends on the domain layer
problem: v3 split MediatR usage across solution-command-integration (writes) and solution-query-integration (reads, bundled with persistence), with command-integration depending on solution-domain-behaviour. v3.1 makes the MediatR mechanism a common feature and the domain layer optional — that split no longer fits.
decision: One common solution-mediator-integration owns the whole pattern — ICommand/IQuery/INotificationEvent markers, request/handler/validator conventions, DI, App.Host wiring — with no dependency on the domain layer. The repository-backed read side stays a separate VP2 solution (solution-query-integration).
tags:
  - solution/mediator-integration
  - stack/dotnet
  - concern/documentation
  - concern/documentation/adr
---

# Problem

The v3.1 Feature Model makes `MediatorModuleIntegration` a **common** feature ("every interaction is a Command/Query dispatch or a Notification") and makes `DomainLogic` (VP1) and `Persistence` (VP2) optional. The v3 arrangement does not fit:

- `solution-command-integration` `depends_on solution-domain-behaviour` — but in v3.1 a module can use commands with no domain layer.
- `solution-query-integration` is bundled into persistence and `depends_on solution-repository-integration` — but the *marker and dispatch* for a query is common; only the *repository-backed handler* needs persistence.
- There was no notification/pub-sub solution at all, though the feature mentions it.

# Selected variant

**Selected variant:** [[#One common solution-mediator-integration, no domain dependency; repository-backed reads stay VP2]]

# Searched variants

## Keep v3's split (command-integration + query-integration), just rename

### Description
Two solutions: `solution-command-integration` (common) and `solution-query-integration` (VP2). Add a third for notifications.

### Benefits
- Minimal churn from v3.
- Each solution is small.

### Costs
- The `ICommand`/`IQuery` markers, the handler/validator/feature-folder convention, the module DI registration, and the App.Host wiring are **identical** for commands and queries — splitting the solution duplicates all of that guidance and risks the two drifting.
- Three solutions for one pattern; an agent applying "MediatR" has to find and reconcile all three.
- `command-integration`'s `depends_on solution-domain-behaviour` has to be dropped anyway, so the "command" solution is no longer really about writes-with-a-domain.

## One common solution-mediator-integration, no domain dependency; repository-backed reads stay VP2 (selected)

### Description
`solution-mediator-integration` (common, rename of `solution-command-integration`) owns the entire pattern: `ICommand`/`ICommand<T>`, `IQuery<T>`, `INotificationEvent` in `Shared`; records in `{Module}.Interfaces/{Commands,Queries,Events}`; one handler/validator convention; module DI self-registration; App.Host wiring. It has **no** `depends_on` on `solution-domain-behaviour` — a handler delegates to the domain layer *when one exists*, stated in `# Boundaries`. The repository-backed query handler, `App.Queries` cross-module read models, and `DbContext` reads remain in `solution-query-integration` (VP2), which builds on this one.

### Benefits
- One solution, one place, for "this family uses MediatR" — matches the single common feature.
- The command/query/notification conventions are written once and cannot drift.
- The dependency graph is honest: the mechanism depends only on `Shared` + the validation solutions; persistence and domain are separate, optional, and downstream.
- Notifications get a real home.

### Costs
- `solution-mediator-integration` is a larger solution than any of the three it replaces.
- `solution-query-integration` (VP2) now has a `depends_on` on `solution-mediator-integration` for the markers it reuses — one more edge, but a truthful one.
- The rename touches every v3 reference to `solution-command-integration` (handled in the v3.1 migration).
