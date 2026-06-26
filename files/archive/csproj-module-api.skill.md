---

name: csproj-module-api 
description: defines the Api project boundary, its structure, API surface selection rules, and what belongs inside it 
domain: skill 
type: csproj 
tags:
- skill/pattern/csproj
- dotnet
- api
- aspnet-core
- module 
triggers:
- create api project
- structure api layer
- add endpoint to module

---

# Goal

Define what the `{ModuleName}.Api` project is, what it contains, and how to select the correct API surface for each operation. The Api layer is a thin HTTP adapter over MediatR — it maps HTTP input to commands or queries, dispatches them, and maps results to HTTP responses. No logic lives here.

# Core Principles

- Api layer is a thin MediatR adapter — no business logic, no validation logic, no persistence
- Each endpoint dispatches exactly one Command or Query
- Entity lifecycle operations use Controllers — system and cross-entity operations use Minimal API
- Every `ResultStatus` from a handler has an explicit documented HTTP mapping
- Unexpected result statuses throw an exception — never return an undocumented response

# Structure

## Solution place

Defined in `csproj-module-layer.skill.md`

```
/src/Modules/{ModuleName}
  /{ModuleName}.Api
```

## Structure

```
/{ModuleName}.Api
  /Controllers
    /{EntityName}
      {EntityName}Controller.cs
      Single{EntityName}Controller.cs
      Single{EntityName}{PropertyName}Controller.cs
      /{RelatedEntity}
        {EntityName}{RelatedEntity}Controller.cs
        Single{EntityName}{RelatedEntity}Controller.cs
  /MinimalApi
    {Domain}Endpoints.cs
  {ModuleName}.Api.csproj
```

## Directory and class skills

|Directory|File|Description|Skill|
|---|---|---|---|
|/Controllers/{EntityName}|{EntityName}Controller.cs|Collection controller|class-api-controller.skill.md|
|/Controllers/{EntityName}|Single{EntityName}Controller.cs|Single entity controller|class-api-controller.skill.md|
|/Controllers/{EntityName}|Single{EntityName}{Property}Controller.cs|Property controller|class-api-controller.skill.md|
|/Controllers/{EntityName}/{Related}|{EntityName}{Related}Controller.cs|Sub-resource controller|class-api-controller.skill.md|
|/MinimalApi|{Domain}Endpoints.cs|System / cross-entity endpoints|class-api-minimal-endpoint.skill.md|

## What Does NOT Belong Here

- Business logic — belongs in Domain
- Validation logic — belongs in Application validator
- DbContext, repositories, EF queries — belong in App.Infrastructure
- Domain rules or state transitions — belong in Domain

## Allowed Dependencies

```
{ModuleName}.Api → {ModuleName}.Interfaces
```

# API Surface Selection

|Operation type|Surface|Example|
|---|---|---|
|Entity collection (create, list)|Controller|`POST /task`|
|Single entity lifecycle (get, update, delete)|Controller|`GET /task/{id}`|
|Addressable entity property|Controller|`POST /task/{id}/is-complete`|
|Related entity collection|Controller|`GET /task/{id}/tag`|
|Relationship instance|Controller|`DELETE /task/{taskId}/tag/{tagId}`|
|System orchestration|Minimal API|health checks, sync jobs|
|External integration / webhook|Minimal API|`POST /webhooks/github`|
|Cross-aggregate operation|Minimal API|spans multiple entities|

# Controller Naming Rules

|Controller type|Pattern|Example|
|---|---|---|
|Collection|`{Entity}Controller`|`TaskController`|
|Single entity|`Single{Entity}Controller`|`SingleTaskController`|
|Property|`Single{Entity}{Property}Controller`|`SingleTaskIsCompleteController`|
|Sub-resource collection|`{Entity}{Related}Controller`|`TaskTagController`|
|Relationship instance|`Single{Entity}{Related}Controller`|`SingleTaskTagController`|

# Rules

MUST:

- Each endpoint dispatches exactly one Command or Query
- Every `ResultStatus` has an explicit `ProducesResponseType`
- Unexpected `ResultStatus` throws `InvalidOperationException`
- Controllers used for entity lifecycle operations
- Minimal API used for system, webhook, batch, cross-aggregate operations MUST NOT:
- Contain business logic, validation logic, or persistence logic
- Dispatch multiple MediatR requests from one endpoint
- Reference App.Infrastructure or App.Queries
- Reference `{ModuleName}.Domain` or `{ModuleName}.Application` directly — only Interfaces

# Anti-patterns

- Controller action contains `if/else` business decisions — belongs in domain
- Controller queries DbContext directly — belongs in handler
- One endpoint dispatches two commands sequentially — design as orchestrating command

# Checklist

- [ ] All entity lifecycle endpoints use Controllers
- [ ] All system/webhook/cross-aggregate endpoints use Minimal API
- [ ] Controller names follow naming rules
- [ ] Each endpoint has one Command or Query
- [ ] Every ResultStatus has ProducesResponseType
- [ ] No business logic in any endpoint
- [ ] No App.Infrastructure or App.Queries reference

# Relations

- class-api-controller.skill.md — how to implement a Controller
- class-api-minimal-endpoint.skill.md — how to implement a Minimal API endpoint
- csproj-module-layer.skill.md — Api is one of four module projects
- solution-command-handling.skill.md — full pipeline this layer sits at the top of