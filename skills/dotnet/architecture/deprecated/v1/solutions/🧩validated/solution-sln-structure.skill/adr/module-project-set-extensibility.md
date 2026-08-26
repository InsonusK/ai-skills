---
name: module-project-set-extensibility
description: The four-project module structure (Api, Application, Domain, Interfaces) is the guaranteed base set; a specific pattern solution may add an extra project when it needs project-level isolation, instead of the count being a fixed ceiling.
problem: solution-sln-structure declared "exactly four projects per module" as a hard MUST rule. This blocks any future pattern solution from introducing a project-level artifact (for example, isolating a specific concern into its own project for testing or dependency-boundary reasons) without either silently violating the rule or forcing a rewrite of the foundation solution for every such need.
decision: Change the rule from "exactly four projects" to "at least the base four projects (Api, Application, Domain, Interfaces)"; a specific pattern solution may add an additional project to a module when it needs project-level isolation. solution-sln-structure does not enumerate which optional projects currently exist — that is discoverable through the solution dependency graph (depends_on), not through this foundation solution's own text.
tags:
  - solution/sln-structure
  - concern/documentation
  - concern/documentation/adr
---

# Problem

`solution-sln-structure` is the foundation solution — every other solution in the catalog depends on it, directly or indirectly — and its own Goal claims to be "a single reference for file placement decisions across the entire solution" and to "define the complete set of allowed and forbidden dependencies between all layers." Its Rules stated a fixed count: every module has exactly four projects (Api, Application, Domain, Interfaces).

That fixed count blocks a legitimate, recurring need: a specific pattern solution occasionally requires project-level isolation for one of its concerns — for example, isolating pure business-rule code into its own project so mutation testing can scope cleanly to it, without pulling in Entities and other Domain-layer code. Under the "exactly four" rule, a pattern solution introducing such a project either silently violates the foundation's own MUST rule, or the foundation has to be rewritten by name every time a new pattern needs this.

# Selected variant

**Selected variant:** [[#Base set, extensible by pattern solutions (selected)]]

`solution-sln-structure` keeps Api, Application, Domain, Interfaces as the guaranteed base every module has. A specific pattern solution may introduce an additional project for a module when it genuinely needs project-level isolation — this is a general permission recorded once in `solution-sln-structure`, not a named registry of every optional project that currently exists. Which optional projects actually exist in the catalog is discoverable via the solution dependency graph (`depends_on` links between solution skills), not by reading `solution-sln-structure`'s own text.

# Searched variants

## Keep "exactly four" as a fixed ceiling

### Description
Leave the MUST rule as "exactly four projects" with no extension mechanism.

### Benefits
- Simplest possible rule — unambiguous, trivial to check.
- No risk of ungoverned project sprawl.

### Costs
- Blocks any legitimate pattern-level need for project isolation without either contradicting the foundation or forcing every such need to be crammed into one of the existing four projects, even when that hurts testability or dependency-boundary clarity.
- Forces a choice between violating the rule silently or rewriting the foundation solution for every new pattern that needs isolation.

## Explicit named registry in solution-sln-structure

### Description
`solution-sln-structure` enumerates every currently-existing optional project by name, alongside which solution creates it, keeping itself a literal, complete list of the whole project graph.

### Benefits
- `solution-sln-structure` remains a single, literal source of truth for the entire current project list — nothing is left to be inferred from elsewhere.

### Costs
- Couples the foundation solution to every specific pattern solution's decisions — it must be edited every time any pattern solution adds a project, even though it doesn't own that pattern's content.
- Duplicates information that already lives in the dependency graph, with a real risk of going stale if a future edit forgets to update the registry.
- Blurs ownership: the foundation would describe content it doesn't create or maintain.

## Base set, extensible by pattern solutions (selected)

### Description
`solution-sln-structure` defines the guaranteed base (Api, Application, Domain, Interfaces) and a general rule permitting a specific pattern solution to add a project when it needs project-level isolation. The foundation does not name which optional projects currently exist — that is answered by the solution catalog's own dependency graph.

### Benefits
- Keeps the foundation solution stable and free of churn caused by unrelated pattern decisions.
- Unblocks legitimate project-level isolation without weakening the guaranteed base every module can rely on.
- Matches how content-level additions already work in this catalog: solutions add classes/behaviors inside existing projects (e.g. BuildingBlocks) without ever touching solution-sln-structure; this extends the same principle one level up, to whole projects.

### Costs
- `solution-sln-structure` alone no longer answers "what is the full current project list for a fully-built module" — the reader has to consult the solution catalog / dependency graph as well.
