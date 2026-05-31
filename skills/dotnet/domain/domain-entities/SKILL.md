---
name: domain-entities
description: Defines domain entity rules.
metadata:
  domain: dotnet
  tags:
    - dotnet
    - domain
    - domain-entities
---
## Entities

Entities represent business concepts.

Entities:

* contain business rules
* protect invariants
* avoid public mutable state

## Rules

Prefer methods over property mutation.

Correct:

task.Complete()

Avoid:

task.Status = Completed

## Persistence

Entities must not contain EF Core specific logic.

Avoid:

* DbContext usage
* IQueryable exposure
* persistence annotations when possible
