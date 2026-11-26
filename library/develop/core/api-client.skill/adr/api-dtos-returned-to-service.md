---
name: api-dtos-returned-to-service
description: Where provider responses become consumer entities
problem: Should the client map a response into application values when one response is enough, or should all consumer-model mapping belong to service?
decision: Client returns API DTOs and technical outcomes; service maps every response into consumer models through domain APIs.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Should the client map a response into application values when one response is enough, or should all consumer-model mapping belong to service?

# Selected variant
[API DTOs mapped by service](#api-dtos-mapped-by-service)

Owner decision, 2026-11-26.

# Searched variants

## API DTOs mapped by service

**Selected.**

### Description
Client parses the provider response into an API-shaped DTO; service adapts that DTO, composing calls of its external system where necessary, and constructs consumer entities through domain APIs. This supersedes the selection in [client-returns-application-values](./client-returns-application-values.md), including its prohibition on response DTOs reaching service. API DTOs do not escape service into commands or domain logic. DTO representation may use a generated type or a client-owned API-shaped type; stack-specific representation is decided during propagation, without restoring consumer-model mapping in client.

### Benefits
- One explicit boundary owns adaptation, even when a single response suffices.
- Clients remain independent of consumer domain semantics.

### Costs
- Service sees provider DTO shapes and changes when they change.
- Existing client specifications, examples and tests need a coordinated migration.

## Client maps one response

### Description
Client returns application values when derivable from one response; service composes multi-call answers.

### Benefits
- Simple conversions can avoid a second mapping.

### Costs
- Mapping ownership depends on how many calls the provider exposes.

## Client builds consumer entities

### Description
Client constructs application entities and returns them directly.

### Benefits
- Service can be very small.

### Costs
- Transport code depends on domain construction and application decisions.
