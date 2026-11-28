---
name: protocol-neutral-client-contract
description: Keep service unchanged when the logical API is served through another protocol
problem: Returning generated protocol DTOs or raw transport failures makes service depend on HTTP or gRPC despite the intended protocol boundary.
decision: Client exposes client-owned protocol-neutral API DTOs and normalized technical outcomes; service adapts the logical API into Domain Model operations.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The owner requires protocol replacement (HTTP to gRPC) for the same logical API
not to change service. The earlier DTO decision left generated messages versus
client-owned API DTOs open. Transport-specific generated signatures, HTTP statuses and gRPC
exceptions cannot satisfy that requirement at the service boundary.

# Selected variant
[Client-owned logical API contract](#client-owned-logical-api-contract)

Owner requirement, 2026-11-28. This resolves the DTO representation left open by
[api-dtos-returned-to-service](./api-dtos-returned-to-service.md); consumer-model
mapping remains in service.

# Searched variants

## Client-owned logical API contract

**Selected.**

### Description
Client exposes its own request/response API DTOs and normalized technical outcomes.
They preserve the provider's logical fields, operations and semantic error codes,
without exposing HTTP status enums, gRPC exceptions or generated message types
that change between transport implementations to service. Transport implementations translate wire forms and failures into
this contract. That mechanical mapping is different from API-to-Domain Model
adaptation, which remains in service. Configured authentication, retries and other
technical call policies stay on the client side. DTOs may be generated from a
shared logical schema if the same types and semantics serve both transports;
handwritten duplicate types are not required merely because generation is used.

Protocol replacement preserves service only when logical operations, DTO meaning
and error semantics stay equivalent. If the provider also changes those, it is an
API change: client declarations and service realization may change while the
consumer-facing contract stays stable if the capability can still be implemented.
Splitting APIs inside one system does not itself create a second system.

### Benefits
- Service code, imports and tests remain unchanged for an equivalent protocol swap.
- Service alone owns consumer-model adaptation and API operation composition.
- Protocol-specific failure handling has one home.

### Costs
- Transport-specific generated wire messages may need an additional mechanical conversion.
- Logical API DTOs and operation contracts need maintenance alongside wire contracts.

## Protocol-specific messages returned directly

### Description
Expose generated gRPC messages or HTTP response models and raw protocol errors
to service, allowing each stack to pick its representation.

### Benefits
- Fewer intermediary types and conversions.

### Costs
- Protocol replacement changes service signatures, imports and error handling.

## One combined client and service

### Description
Put endpoint calls, protocol policies, API composition and Domain Model mapping
in one integration class.

### Benefits
- Fewer explicit boundaries and thin types in simple integrations.

### Costs
- Protocol and API adaptation change together and cannot be tested independently.
- Repeated calls and policies tend to leak into consumer-specific operations.
