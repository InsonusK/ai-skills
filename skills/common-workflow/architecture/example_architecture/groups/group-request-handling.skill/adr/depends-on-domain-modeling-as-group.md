---
name: depends-on-domain-modeling-as-group
description: Whether group-request-handling should declare depends_on for each individual domain-modeling solution it touches, or one edge on the group as a whole
problem: Both solution-command-handler and solution-transport-validation eventually need something from group-domain-modeling (an entity method, a value object). Deciding this per solution risks repeating the mistake already made once in the real .NET catalog.
decision: group-request-handling declares depends_on group-domain-modeling once, at the group level. No solution inside group-request-handling repeats that edge individually.
tags:
  - group/request-handling
  - stack/dotnet
  - concern/documentation
  - concern/documentation/adr
---

# Problem

`solution-command-handler` loads an entity and calls a guarded method on it; `solution-transport-validation` checks input shape before the handler runs. Both solutions therefore "touch" domain-modeling concepts in some sense. The question is where to record that: on each solution individually, or once on the group.

This is not a hypothetical risk. In the real .NET solution catalog this repository maintains (`skills/dotnet/architecture/deprecated/v1/solutions/`), `solution-validation-behavior` carried a `depends_on` on `solution-repository-integration`, justified as "handlers return `Result<T>` via Ardalis patterns". That justification was wrong: `Result<T>` comes from the `Ardalis.Result` package, which `solution-validation-behavior` already depends on directly — `solution-repository-integration` is about a different package (`Ardalis.Specification`) entirely and never mentions `Result<T>`. The edge existed only because the two solutions "run in the same request lifecycle", not because one requires the other, and it went unnoticed until an explicit review.

# Selected variant

**Selected variant:** [[#One group-level depends_on edge (selected)]]

# Searched variants

## One group-level depends_on edge (selected)

### Description

`group-request-handling` declares `depends_on: [group-domain-modeling]` once, in its own frontmatter. `solution-command-handler` and `solution-transport-validation` declare no `depends_on` on any domain-modeling solution at all.

### Benefits

- One edge to read to understand the relationship between the two groups, instead of reconstructing it from N solution-level edges.
- A new solution added later to either group inherits the correct dependency automatically — there is nothing to remember to add.
- Removes the specific failure mode that produced the real `solution-validation-behavior` → `solution-repository-integration` edge: there is no per-solution slot to fill in "by association" in the first place.

### Costs

- Loses precision: reading the group edge alone does not say *which* domain-modeling solution a given handler actually calls. An agent implementing one specific handler still has to open the handler's own `Implementation/` file to find out.

## Per-solution depends_on edges

### Description

Each solution inside `group-request-handling` declares its own `depends_on` on the specific domain-modeling solution(s) it needs — e.g. `solution-command-handler` depends on `solution-entity-invariant`.

### Benefits

- More precise at the solution level: the edge names exactly which mechanism is used.

### Costs

- Multiplies edges: every solution added to either group needs its own correct edge, and nothing forces it to be correct — this is the exact shape of the real, observed mistake described in `# Problem`.
- Two solutions in the same group can end up with inconsistent dependency sets (one lists the domain edge, a sibling forgets it) even though both need the same guarantee from the other group.

## No declared dependency at all

### Description

Neither the group nor its solutions declare any relationship to `group-domain-modeling`; the connection is left implicit in the handler's code.

### Benefits

- No frontmatter to maintain.

### Costs

- An agent assembling a plateau or reasoning about apply order has no signal that `group-request-handling` requires `group-domain-modeling` to exist first.
- The relationship is discoverable only by reading every `Implementation/` file's code, not by reading any skill header.
