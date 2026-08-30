---
name: registry-dto-validator-cs
description: Conflict Detection result for the `{ValueObject}PropertyValidator.cs` and `{Dto}.Validator.cs` elements
tags:
  - concern/architecture
  - stack/dotnet
  - element/dto-validator-cs
  - element/valueobject-propertyvalidator-cs
---

# Element
`{ValueObject}PropertyValidator.cs` and `{Dto}.Validator.cs` (both in `{Module}.Application`) — grouped in one entry because both follow the identical pattern below.

# Involved solutions
- [[skills/dotnet/architecture/v3/solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] (`.create` — base validators, each with a locally-owned `Must(...)` condition)
- [[skills/dotnet/architecture/v3/solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] (`.extend` — replaces each local `Must(...)` with a call to the same centralized condition the corresponding Entity/Value Object now calls, once duplication was observed)

# Classification
Same shape as [[skills/dotnet/architecture/v3/plateau/plateau-shared-rules/registry/valueobject-cs.md|the `{ValueObject}.cs` entry]]: ordering guaranteed by `built_on_plateau` (`solution-dto-property-validators` is core to `plateau-service-with-validated-module-interaction`; `solution-domain-rules` is built on top of that same plateau), Category `M`, Kind `N` — the redirect only ever touches the one condition it names, never the validator's other rules.

# Ordering
`source: constraint`, free from `built_on_plateau` — same reasoning as [[skills/dotnet/architecture/v3/plateau/plateau-shared-rules/registry/valueobject-cs.md|the `{ValueObject}.cs` entry]].

# Resolution
Canonical — no resolver needed, for the same reason as [[skills/dotnet/architecture/v3/plateau/plateau-shared-rules/registry/valueobject-cs.md|the `{ValueObject}.cs` entry]]: a targeted redirect of an already-isolated condition, not a parallel addition.
