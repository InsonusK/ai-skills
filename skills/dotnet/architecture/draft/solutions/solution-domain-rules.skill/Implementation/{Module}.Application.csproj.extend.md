---
description: Add a project reference to {Module}.Domain.Rules and redirect already-existing local conditions in the validators to it
project_name: "{Module}.Application"
name: "{Module}.Application.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/domain-rules
  - element/module-application-csproj
---

# Goals
- Point `{Module}.Application`'s property/DTO/async validators at the newly-centralized `{Module}.Domain.Rules`, once a duplicated condition has been found

# Rule changes

## MUST
- Add a project reference to `{Module}.Domain.Rules`

## MUST NOT
- Redirect a condition that is not actually duplicated elsewhere
