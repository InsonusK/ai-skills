---
name: minimal-api
description: Defines Minimal API conventions.
metadata:
  domain: dotnet
  tags:
    - dotnet
    - minimal-api
---
## Endpoints

Endpoints must remain thin.

Endpoints:

* receive HTTP requests
* map DTOs
* call MediatR
* return standardized responses

Avoid:

* business logic inside endpoints
* DbContext usage
* direct EF queries

## Error Handling

Use ProblemDetails.

Use centralized exception handling.

## Versioning

Support explicit API versioning when public APIs exist.
