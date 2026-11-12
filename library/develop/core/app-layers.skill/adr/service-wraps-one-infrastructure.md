---
name: service wraps one infrastructure
description: How work with storage and other systems is split between domain services, services, and infrastructure, and which of them may call which
problem: Commands and domain services need storage and other systems, but the technique of each (SQL, HTTP calls, transactions) must not spread into them; where does that technique stop, and how do the layers that use it call each other?
decision: An infrastructure adapter holds the technique; exactly one service wraps it with methods in the application's language; domain services hold reusable business logic and may call services; commands call domain services, entities, and services; no service calls another service; services and infrastructure are reached through interfaces.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
In tg-bot-service, commands reached stores and clients directly, and one rule ended up implemented twice, differently. The owner's requirements (2026-10-17): api parses and calls a command; a command takes plain parameters; a command never touches infrastructure — infrastructure is wrapped by a service, so replacing HTTP by gRPC or PostgreSQL by Redis changes the infrastructure and at most the service, never the application's logic. The question is how far the wrapping goes and who may call whom.

# Selected variant
[[#Service per infrastructure, domain services above it]]
- Owner decision, 2026-10-17: a domain service holds reusable business logic and may call services for nested operations; a simple command may call a service directly.

# Searched variants

## Service per infrastructure, domain services above it

**Selected.**

### Description
Infrastructure (`UserHTTPClient`, a PostgreSQL store) does the technical work. One service per infrastructure (`UserService`, `StorageService`) offers methods in the application's language and hides the calls and the transaction. A domain service (`Registration.Register`: check, add, save the link) composes services and entities into reusable logic. A command calls domain services, entities, and services. A service never calls another service. Callers reach services, and services reach infrastructure, through interfaces.

The same split under older names: in `solution-integration-client-layering` and the plateau catalogs built on it, **Client** is what this skill calls a **service** (composes calls into the application's own model) and **Integration** is what it calls **infrastructure** (calls the API, maps its response, rejects a contract mismatch). The older skill keeps its names until those catalogs are replaced; code written to this skill uses service and infrastructure.

### Benefits
- Replacing a store or a transport touches one adapter and one service.
- A simple command stays simple: it calls the service it needs, with no domain service in between.
- Each layer is tested with only the layer below faked.

### Costs
- One more type per infrastructure (the service), some of them thin.
- A domain service that calls services is no longer pure; its tests need fake services.

## Domain services own the infrastructure interfaces (tg-bot-service)

### Description
Domain services in `domain/services` wrap the interfaces of `domain/interfaces` directly; there is no separate service layer.

### Benefits
- Fewer types; one service per meaning.

### Costs
- Business logic and the hiding of technique live in the same type, so a change of storage edits the type holding the rules.
- The domain package declares what is stored and sent.

## Commands call infrastructure directly

### Description
No services; commands use the clients and stores.

### Benefits
- Nothing between a command and the system it uses.

### Costs
- Every command repeats the technique; no store or transport can be replaced or faked without editing commands.
