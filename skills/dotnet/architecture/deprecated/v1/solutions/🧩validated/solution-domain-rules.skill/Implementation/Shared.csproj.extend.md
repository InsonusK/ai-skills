---
description: Add EntityNotLoadedException to Shared — thrown when an Entity method needs a navigation the caller did not load
project_name: "Shared"
name: "Shared.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/domain-rules
  - element/shared-csproj
---

# Goals
- Distinguish "the Handler forgot to load a required navigation" (a Handler defect) from "the request itself is invalid" (`DomainException`), so the two never share an HTTP status or a test expectation

# Rule changes

## MUST
- Add `EntityNotLoadedException` to `Shared.Exceptions`

## MUST NOT
- Map `EntityNotLoadedException` to the same 4xx path as `DomainException`
