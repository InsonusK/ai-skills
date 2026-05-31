---
name: feature-based-architecture
description: Defines feature-based organization rules.
metadata:
  domain: dotnet
  tags:
    - dotnet
    - architecture
    - architecture-feature-based
---
## Feature Organization

Each feature must be self-contained.

Correct:

```
Features/
  CreateTask/
    Command.cs
    Handler.cs
    Validator.cs
    Endpoint.cs
    Mapping.cs
```

Avoid:

```
Handlers/
Validators/
Repositories/
```

## Rules

* Keep all feature files together
* One handler per feature
* Validation must live near the command/query
* Feature folders must remain small and focused

## Naming

CreateTaskCommand
CreateTaskHandler
CreateTaskValidator

Consistency is mandatory.
