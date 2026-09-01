---
description: Add a project reference to {Module}.Domain.Rules and redirect already-existing local conditions in {ValueObject}.cs/{EntityName}.cs to it
project_name: "{Module}.Domain"
name: "{Module}.Domain.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/domain-rules
  - element/module-domain-csproj
---

# Goals
- Point `{Module}.Domain`'s VO and Entity conditions at the newly-centralized `{Module}.Domain.Rules`, once a duplicated condition has been found

# Rule changes

## MUST
- Add a project reference to `{Module}.Domain.Rules`
- Never redirect a condition that is not actually duplicated elsewhere — a condition used by exactly one consumer stays local, per `solution-value-objects`/`solution-domain-behaviour`'s own Boundaries

