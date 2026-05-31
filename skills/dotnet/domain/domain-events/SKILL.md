---
name: domain-events
description: Defines domain event conventions.
metadata:
  domain: dotnet
  tags:
    - dotnet
    - domain
    - domain-events
---
## Domain Events

Domain events represent important business events.

Examples:

* TaskCompletedEvent
* UserRegisteredEvent

## Rules

Domain events:

* must be immutable
* must describe past events
* must not contain infrastructure logic

## Usage

Entities raise domain events.

Handlers react to them.

Avoid direct side effects inside entities.
