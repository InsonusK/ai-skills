---
description: Add ChangeCustomerEmailCommand and its handler to {Module}.Application
name: "{Module}.Application"
element_kind: project
change_kind: extend
tags:
  - solution/command-handler
  - element/module-application-csproj
---

# Structure

## Directory and class skills
| Directory | file | Description |
| --- | --- | --- |
| /Commands | ChangeCustomerEmailCommand.cs | Immutable write-intent record |
| /Commands | ChangeCustomerEmailHandler.cs | Loads `Customer`, calls its guarded method |
