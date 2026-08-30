---
name: registry-entityname-cs
description: Conflict Detection result for the `{EntityName}.cs` element
tags:
  - concern/architecture
  - stack/dotnet
  - element/entityname-cs
---

# Element
`{EntityName}.cs` (`{Module}.Domain/{EntityName}.cs`)

# Involved solutions
Six solutions carry `element/entityname-cs`, but they are not six equal participants — reading each `.extend.md`'s `## MUST`/`Implementation changes` sections separates them into three groups:

1. [[skills/dotnet/architecture/v3/solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] (core, always present) → [[skills/dotnet/architecture/v3/solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] — a sequential redirect, same shape as [[skills/dotnet/architecture/v3/plateau/plateau-shared-rules/registry/valueobject-cs.md|the `{ValueObject}.cs` entry]]: `solution-domain-rules` replaces a locally-owned `private static` guard `solution-domain-behaviour` established, and states this explicitly in its own "Before" section.
2. [[skills/dotnet/architecture/v3/solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] (adds `Version`/`IVersioned`, Mutable kinds only), [[skills/dotnet/architecture/v3/solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] (adds `Guid`, External kinds only), and [[skills/dotnet/architecture/v3/solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] (adds timestamp properties/interfaces, classification-dependent) — three independent property additions, each gated by [[skills/dotnet/architecture/v3/variability-map.md|VP1 (EntityKind)]] and none claiming a shared name or position (unlike `{Command}.cs`'s Guid/ActionTimeStamp clash — see [[skills/dotnet/architecture/v3/plateau/plateau-statefull-service/registry/command-cs.md|that entry]] — none of these three states a "first property" requirement on the *entity* class itself, only the corresponding *command* extensions do).
3. [[skills/dotnet/architecture/v3/solutions/solution-entity-classification.skill/solution-entity-classification.skill.md|solution-entity-classification]] — documentary/selector, not an independent code contribution: its own `Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend.md` restates the combined shape group 2 already produces per classification variant, and its YAML `depends_on` names both `solution-entity-concurrency-change` and `solution-external-created-entity` directly — it does not add a property neither of those two already adds.

# Classification
- Group 1 (`solution-domain-behaviour` → `solution-domain-rules`): same as [[skills/dotnet/architecture/v3/plateau/plateau-shared-rules/registry/valueobject-cs.md|`{ValueObject}.cs`]] — sequential, Category `M`, Kind `N`, ordering free from `built_on_plateau` (`source: constraint`).
- Group 2 (concurrency-change / external-created-entity / edit-timestamp, pairwise): `FMN`. Constraint: `F` between each pair (no `depends_on` linking any two of them). Category: `M`. Kind: `N` — each adds a distinct, non-overlapping property/interface set; classification (VP1) only decides *which* combination of them applies to a given entity, it does not make their code changes collide when more than one applies.
- `solution-entity-classification`'s own entry: not classified as a separate intersection — it is evidence *for* Group 2's classification, not a fourth contributor to it.

# Ordering
Group 1: `source: constraint`, free from `built_on_plateau` (same as `{ValueObject}.cs`). Group 2: no ordering requirement exists or is needed between the three — they can apply in any order relative to each other, only their own individual application condition (which VP1 variant is selected) matters.

# Resolution
Canonical throughout — no resolver needed anywhere in this group.

# Architectural signal
Four genuinely independent contributors touch this one class (`solution-domain-behaviour`, `solution-domain-rules`, and up to three of {`solution-entity-concurrency-change`, `solution-external-created-entity`, `solution-entity-edit-timestamp`} depending on the entity's classification) — N≥3 by the parent skill's rule, even though every pairing resolves canonically. Worth reviewing later whether `{EntityName}.cs`'s per-classification infrastructure properties (`Guid`, `Version`, timestamps) are accumulating enough independent extend-files that a single, classification-driven partial class or composed marker-interface set would be clearer than four separate `.extend.md` files converging on one class — not urgent today since none of them actually conflict, but the same shape that produced a real conflict on `{Command}.cs` (see [[skills/dotnet/architecture/v3/plateau/plateau-statefull-service/registry/command-cs.md|that entry]]) is present here in a currently-benign form.
