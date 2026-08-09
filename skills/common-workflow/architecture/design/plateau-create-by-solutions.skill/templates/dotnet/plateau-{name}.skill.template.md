---
name: plateau-name
description: Describe which plateau does skill describe
whenToUse: One concrete sentence — which task must make the agent read this plateau before writing code
  # MUST name a concrete situation: starting a new repository/module/feature under this plateau, or checking whether a change follows it. MUST NOT be vague ("when relevant").
  # Example: "when scaffolding a new module, or reviewing whether a change follows command-query separation and optimistic concurrency"
domain: skill
type: template
version:
tags:
  - skill/template/plateau
parent_plateau:
created_by:
---
# How Apply this template
- Fill `whenToUse` with the concrete situations that should make the agent read the plateau before writing code (starting a new repository/module/feature under `{plateau-name}`, or checking whether a change already made follows it). See [skill-design](skills/common-workflow/skill-design.skill/skill-design.skill.md) for the baseline rules.
- add to header properties `tags` tag `plateau/{plateau-name}`

# Goal
```hint
Describe the purpose of this plateau.

MUST:
- If `parent_plateau` is set, explain what problem the solutions in `created_by` solve or what behavior they introduce on top of the parent plateau.
- If `parent_plateau` is empty, explain the overall purpose of the plateau.

RECOMENDATION:
- Keep it to one or two sentences.
```
```example
Add command-query separation and optimistic concurrency control on top of the default .NET solution structure.
```

# Core Principles
```hint
Summarise core principles introduced or changed by the solutions in `created_by`.

MUST:
- If `parent_plateau` is set, describe only the delta relative to the parent plateau.
- If Core Principles conflict with each other, ask the user to resolve the problem.
- Don't just copy principles; make a brief summary.

RECOMENDATION:
- Prefer bullet list.
```
```example
- Validation: entity version is validated on every change.
```

# Capabilities
```hint
What capabilities does this plateau add or change.

MUST:
- If `parent_plateau` is set, describe only the delta relative to the parent plateau.
- If Capabilities conflict with each other, ask the user to resolve the problem.
- Summarize capabilities from the solutions in `created_by` and group them logically.

RECOMENDATION:
- Prefer bullet list.
```
```example
- workflow
	- All commands and queries are validated by FluentValidator in `ValidationBehavior`.
- validation
	- All modules validate DTOs and soft value objects from other modules using their validators.
```

# Usecases
```hint
Fill use cases that demonstrate new or changed interactions introduced by this plateau.

MUST:
- If `parent_plateau` is set, focus on scenarios that are added or changed relative to the parent plateau.

RECOMENDATION:
- Include examples of interactions and cron jobs if applicable.
```
## {Case name}
```hint
write short description and mermaid workflow
```
````example
Update entity with concurency check
```mermaid
sequenceDiagram
    participant Client
    participant Api as {Module}.Api
    participant Behavior as ConcurrencyBehavior
    participant Factory as EntityVersionResolverFactory
    participant Resolver as {Entity}VersionResolver
    participant Handler as Update{Entity}CommandHandler

    Client->>Api: GET /{entity}/2
    Api-->>Client: 200 OK + ETag("Task":{"2":3})

    Client->>Api: PUT /{entity}/2 If-Match: <etag>
    Api->>Behavior: Send(command with Versions)
    Behavior->>Factory: GetFor("Task")
    Factory->>Resolver: resolve TaskVersionResolver
    loop for each (id, expectedVersion)
        Behavior->>Resolver: GetCurrentVersionForAsync(2)
        Resolver->>Resolver: FirstOrDefaultAsync({Entity}ByIdSpec)
        Resolver-->>Behavior: 3
        Behavior->>Behavior: assert 3 == expected
    end
    Behavior->>Handler: next()
    Handler-->>Api: Result.NoContent
    Api-->>Client: 204 No Content

    alt version mismatch
        Resolver-->>Behavior: 7
        Behavior-->>Api: Result.Conflict
        Api-->>Client: 409 Conflict
    end
```
````
