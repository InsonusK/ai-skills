---
description: Add Validators/Property, Validators/Model, Validators/Async folders to {Module}.Application and ensure assembly-scan registration
project_name: "{Module}.Application"
name: "{Module}.Application.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/dto-property-validators
  - element/module-application-csproj
---

# Goals
- Give every module a consistent place for property validators, DTO validators, and async cross-aggregate check wrappers

# Rule changes

## MUST
- Add `/Validators/Property`, `/Validators/Model`, `/Validators/Async` folders to `{Module}.Application`
- Call `AddValidatorsFromAssembly` for `{Module}.Application`'s own assembly

## MUST NOT
- Register a validator manually instead of via assembly scan
