---
name: api-structure
description: Defines strict rules for designing ASP.NET Core APIs using entity-centric Controllers and Minimal API for edge/system operations.
metadata:
  domain: dotnet
  tags:
    - dotnet
    - controllers
    - minimal-api
    - cqrs
    - mediatr
    - vertical-slice
---

## Core Principle

API layer is a thin HTTP adapter over MediatR.

It must NOT contain:
- business logic
- validation logic
- persistence logic (DbContext / EF Core)
- domain rules

It MUST only:
- map HTTP → DTO / Query / Command
- dispatch MediatR request
- map result → HTTP response

---

## Architecture Model

Two API surface types exist:

1. Entity-centric Controllers (primary)
2. Minimal API endpoints (system / cross-entity / edge cases)

---

# 1. Controller Design Model (Entity-Centric)

Controllers represent a domain entity or entity relationship boundary.

They define HTTP access to a cohesive domain model.

---

## 1.1 Collection Controller (Entity Root)

Example:
TaskController → /task

Responsibilities:
- manage entity collection
- search/filter/list
- create entities

Endpoints:
- POST /task → CreateTaskCommand
- GET /task → GetTasksQuery

---

## 1.2 Single Entity Controller

Example:
SingleTaskController → /task/{id}

Responsibilities:
- manage lifecycle of a single entity

Endpoints:
- GET /task/{id} → GetTaskQuery
- PUT /task/{id} → UpdateTaskCommand
- PATCH /task/{id} → PatchTaskCommand
- DELETE /task/{id} → DeleteTaskCommand

Optional:
- POST /task/{id}/sync → SyncTaskCommand

---

## 1.3 Property Controller (Entity Sub-resource)

Example:
SingleTaskIsCompleteController → /task/{id}/is-complete

Responsibilities:
- manage single property of entity

Endpoints:
- POST /task/{id}/is-complete → SetIsCompleteCommand
- DELETE /task/{id}/is-complete → UnsetIsCompleteCommand

---

## 1.4 Collection Sub-resource Controller

Example:
TaskTagController → /task/{id}/tag

Responsibilities:
- manage related collection

Endpoints:
- GET /task/{id}/tag → GetTaskTagsQuery
- POST /task/{id}/tag → AddTaskTagCommand

---

## 1.5 Relationship Controller

Example:
SingleTaskTagController → /task/{taskId}/tag/{tagId}

Responsibilities:
- manage specific relationship instance

Endpoints:
- GET /task/{taskId}/tag/{tagId} → GetTaskTagQuery
- PUT /task/{taskId}/tag/{tagId} → UpdateTaskTagCommand
- PATCH /task/{taskId}/tag/{tagId} → PatchTaskTagCommand
- DELETE /task/{taskId}/tag/{tagId} → RemoveTaskTagCommand

Optional:
- POST /task/{taskId}/tag/{tagId}/sync → SyncTaskTagCommand

---

## Controller Naming Rules

- Collection: TaskController
- Single entity: SingleTaskController
- Property: SingleTaskIsCompleteController
- Sub-collection: TaskTagController
- Relationship instance: SingleTaskTagController

---

# 2. Minimal API Design Model

Minimal API is used ONLY for system-level or cross-entity operations.

It MUST NOT represent entity lifecycle operations.

---

## Allowed Use Cases

- system orchestration
- external integrations
- webhooks
- batch processing
- cross-aggregate operations
- infrastructure endpoints

---

## Examples

- POST /webhooks/github
- POST /sync/external-tasks
- GET /health
- POST /batch/recalculate-statistics

---

## Rule

If operation belongs to entity lifecycle → Controller

If operation spans multiple entities or is system-level → Minimal API

---

# 3. Shared API Rules

## 3.1 No Business Logic

Forbidden in API layer:
- calculations
- domain decisions
- state transitions
- validation rules

Allowed:
- request mapping
- MediatR dispatch
- response mapping

---

## 3.2 No DbContext / EF Core

API layer must NOT:
- query database
- use repositories
- use EF Core

All persistence logic lives in Application/Handlers.

---

## 3.3 MediatR Boundary

Each endpoint maps to exactly one:
- Command (write)
- Query (read)

No mixed responsibilities.

---

## 3.4 DTO Mapping Rules

Allowed:
- Route → Query
- Body → Command
- QueryString → DTO

Forbidden:
- business transformations
- domain logic inside mapping

---

## 3.5 Response Rules

All responses must be standardized:
- success → typed DTO
- errors → ProblemDetails
- consistent API contract

---

# 4. Suggested Folder Structure

/Api
  /Controllers
    /Task
      TaskController.cs
      SingleTaskController.cs
      TaskTagController.cs
      SingleTaskTagController.cs
      SingleTaskIsCompleteController.cs

  /MinimalApi
    WebhookEndpoints.cs
    SyncEndpoints.cs
    SystemEndpoints.cs

/Application
  /Features
    /Task
      /CreateTask
      /GetTask
      /UpdateTask
      /DeleteTask
      /SyncTask

---

# 5. Decision Rule

## Use Controller when:
- endpoint belongs to entity lifecycle
- CRUD or entity mutation exists
- relationship is explicitly modeled

## Use Minimal API when:
- endpoint is system-level
- operation spans multiple entities
- integration / orchestration / infrastructure logic

---

# 6. Summary

- Controllers = entity boundary API
- Minimal API = system boundary API
- API layer = thin MediatR adapter
- no business logic in API
- no persistence in API