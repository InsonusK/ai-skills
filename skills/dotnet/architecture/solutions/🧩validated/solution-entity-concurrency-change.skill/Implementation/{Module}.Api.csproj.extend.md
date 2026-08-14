---
description: Add ETag header to GET responses and If-Match guard to PUT/PATCH
name: "{Module}.Api.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/entity-concurrency-change
  - element/module-api-csproj
---

# Goals
- Add ETag header to all GET responses for mutable entities
- Add `If-Match` header extraction, 412 guard, and `Versions` population to all PUT and PATCH endpoints

# Core Principles
- ETag format: `"<base64>"` — surrounding double quotes are part of the HTTP ETag format
- `ETagEncoder.Encode` builds the versions dictionary — entity name string must match `EntityVersionResolverFactory` keys exactly
- If `If-Match` missing or `ETagEncoder.Decode` returns null → return `StatusCode(412)` immediately, before `_sender.Send()`
- `Versions` passed directly as command constructor argument — no manual construction in controller

# Structure

## Project Structure
```
/{Module}.Api
  /Controllers
    Single{Entity}Controller.cs    ← extended with ETag and If-Match handling
```

# Allowed Dependencies
- Shared
- BuildingBlocks
- {Module}.Interfaces

# Rules

## MUST
- GET for mutable entity sets `Response.Headers.ETag` with encoded versions
- PUT/PATCH checks `If-Match` presence — returns 412 if missing or malformed
- `Versions` passed to command from decoded `If-Match` — never constructed in controller
- 412 added to `[ProducesResponseType]` on all PUT/PATCH endpoints for mutable entities
- DTO returned by GET for mutable entity includes `Version` field

## MUST NOT
- GET for immutable entity set ETag header — immutable entities have no version
- `Versions` hardcoded or constructed in controller — always from decoded `If-Match`

# Anti-patterns
- ETag encoding only primary entity version — misses secondary entity conflicts
- Controller returns 400 for missing `If-Match` — 412 Precondition Failed is correct

# Check list
- [ ] GET sets `Response.Headers.ETag`
- [ ] PUT/PATCH checks `If-Match`
- [ ] 412 returned if `If-Match` missing or malformed
- [ ] `Versions` passed to command from decoded `If-Match`
- [ ] 412 declared in `[ProducesResponseType]`
