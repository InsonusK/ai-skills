---
name: registry-valueobject-cs
description: Conflict Detection result for the `{ValueObject}.cs` element
tags:
  - concern/architecture
  - stack/dotnet
  - element/valueobject-cs
---

# Element
`{ValueObject}.cs` (`{Module}.Domain/{ValueObject}.cs`)

# Involved solutions
- [[skills/dotnet/architecture/v3/solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]] (`.create` — base VO with its own locally-owned validation condition)
- [[skills/dotnet/architecture/v3/solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] (`.extend` — replaces the local condition with a call to a centralized `Check()`, per its own `Implementation/{Module}.Domain.csproj.extend/{ValueObject}.cs.extend.md`)

# Classification
`TD-`-shaped, though the ordering here comes from `built_on_plateau` rather than a `depends_on` constraint: `solution-value-objects` is core to `plateau-service-with-validated-module-interaction`, and `solution-domain-rules`' own `built_on_plateau` is that same plateau — so `solution-domain-rules` can only ever apply after `solution-value-objects` already exists. Category: `M` (a real code change, not a DI substitution — the constructor's condition itself is replaced). Kind: `N` (independent in effect — `solution-domain-rules` only ever redirects a condition `solution-value-objects` already isolated behind its own constructor check; it does not touch any other part of the class).

# Ordering
`source: constraint` — free from `built_on_plateau`, not from a `depends_on` entry (`solution-domain-rules`' own `depends_on` is empty; the ordering is guaranteed structurally by which plateau each solution is built on, per [[skills/dotnet/architecture/v3/variability-map.md|the Variability Map]]'s note on VP4).

# Resolution
Canonical — no resolver needed. `solution-domain-rules` is written explicitly as a redirect of an already-isolated condition, never as a parallel, independent addition — so there is nothing for a second delta to collide with.
