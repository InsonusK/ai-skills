---
description: Redirect ChangeCustomerEmailCommandValidator to call EmailRule.IsValid
name: "{Module}.Application"
element_kind: project
change_kind: extend
tags:
  - solution/domain-rule
  - element/module-application-csproj
---

# Structure

## Directory and class skills
| Directory | file | Description |
| --- | --- | --- |
| /Commands | ChangeCustomerEmailCommandValidator.cs | Redirected to call `EmailRule.IsValid` instead of its own `.Must(...)` |
