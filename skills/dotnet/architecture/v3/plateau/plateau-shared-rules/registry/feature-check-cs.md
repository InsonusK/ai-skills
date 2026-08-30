---
name: registry-feature-check-cs
description: Conflict Detection result for the `{Feature}Check.cs` element
tags:
  - concern/architecture
  - stack/dotnet
  - element/feature-check-cs
---

# Element
`{Feature}Check.cs` (`{Module}.Application`)

# Involved solutions
- [[skills/dotnet/architecture/v3/solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] (`.create` — base async check, `Load` throws `NotSupportedException`, `CheckAsync` does a local comparison)
- [[skills/dotnet/architecture/v3/solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] (`.extend` — replaces `CheckAsync`'s local comparison with a call to a centralized, Domain-classified `Check()`)
- [[skills/dotnet/architecture/v3/solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] (`.extend` — replaces `Load`'s `NotSupportedException` with a real `IReadRepository<T>` implementation, now that persistence is composed)

# Classification
`FMN`. Constraint: `F` (no `depends_on` between `solution-domain-rules` and `solution-repository-integration`). Category: `M` (both are real code changes). Kind: `N` (independent) — read both `.extend.md` files: `solution-domain-rules` only ever touches `CheckAsync`'s comparison; `solution-repository-integration` only ever touches `Load`'s body. Two distinct methods on the same class, no shared lines.

# Ordering
No hard constraint gates this pair, but there is a sensible application order worth recording even though nothing requires it: `solution-repository-integration`'s real `Load` implementation is what gives `solution-domain-rules`' centralized `Check()` real data to evaluate — applying `solution-domain-rules` first (while `Load` still throws) would not be *incorrect* (each edits a different method) but would leave `Check()` uncallable until `Load` is also fixed. `source: ordering-only`, and no resolver exists to enforce it — this is a documentation note for whoever is applying both, not a code-level requirement.

# Resolution
Canonical — no resolver needed. Both changes are independent by content (different methods); recorded here only because both solutions are `built_on_plateau`/composed at different points (`solution-dto-property-validators` at `plateau-service-with-validated-module-interaction`, `solution-repository-integration` at `plateau-statefull-service`) and a reader might otherwise assume they interact more than they do.
