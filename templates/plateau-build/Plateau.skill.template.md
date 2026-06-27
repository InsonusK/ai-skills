---
name: plateau-name
description: Describe which plateau does skill describe
domain: skill
type: template
version: 20260615
tags:
  - skill/template/plateau
  #- tag for skill classification
created_by:
  #List of ALL solution which must be implemented in this plateau
  #Example:
  #- "[[link]]"
---

# Core Principals
```hint
Summarise core principles from applied solutions.

MUST:
- If Core Principals conflicted to each other as user to solve the problem
- Don't just copy principles, make brief summary

RECOMENDATION:
- Prefer bullet list
```
```example
- Validation entity version on change
```
# Capabilities
```hint
What capabilities does this plateau has

MUST:
- If Capabilities conflicted to each other as user to solve the problem
- Summaraize all capabilities from all used solutions and logicaly group them

RECOMENDATION:
- Prefer bullet list
```
```example
- workflow
	- All command and queries validate by FluentValidator in `ValidatorBahaviour`
- validation
	- all modules valudate dto and soft{ValueObject} from other module using there validator
```

# Usecases
```hint
fill usecases for plateau
- example of interactions
- example of cron jobs
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