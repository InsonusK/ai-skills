---
name: client returns application values
description: What a client returns to its service, where an answer that needs more than one call or a decision is turned into the application's values, and where that stops
problem: The application should not know how an API answers, but turning an answer into the application's values sometimes needs a decision of the application, a second operation, or data from another system; which of client, service, and the layers above does each, and what does the service no longer hide?
decision: The client returns the application's values when they follow from one answer, otherwise a result type of its own; the service composes any operations of its own client and its parameters into its answer, so a change inside the API edits only the service; data of another infrastructure is joined by a domain service or a command; a change that moves the data to another system - its master system - is a redesign at the level of the project.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
An API `/getUserName` answers `Ann Lee`; the application wants name and surname. Later the provider answers `{nameId} Lee` and adds `/getNameById`. The owner's requirement (2026-10-30): the application knows the service, never how the API answers, so such a change edits the service and not its callers. The question is where each mapping lives, and where hiding a change stops.

# Selected variant
[[#Client maps one answer, service composes its client's operations]]
- Owner decision, 2026-10-30: a change that moves the data to another API changes the master system of that data; it is redesigned at the level of the project, and the callers of the service change with it.

# Searched variants

## Client maps one answer, service composes its client's operations

**Selected.**

### Description
The client returns the application's values when one answer is enough (splitting `Ann Lee`), and a result type of its own otherwise: only the fields the service uses, named in the application's language, nothing of the contract's shape. The service builds its answer from any operations of its client and its own parameters (`/getUserName`, then `/getNameById`). Data of another infrastructure is joined by a domain service or a command, as [service-wraps-one-infrastructure](library/develop/core/app-layers.skill/adr/service-wraps-one-infrastructure.md) requires.

### Benefits
- A change inside one API edits the client and the service, never their callers.
- The client stays free of business rules; replacing the transport keeps them.
- The criterion is checkable: does the value follow from one answer alone?

### Costs
- Two kinds of return value in one client: the application's values and its own result types.

## Client always returns its own result types

### Description
Every client method returns a type of the client; the service maps every value into the application's.

### Benefits
- One kind of return value in every client.

### Costs
- A mapping that needs nothing but the answer is written twice, once into the result type and once into the application's value.

## Client builds the application's entities from what it is given

### Description
The service passes the client the entities it needs, and the client returns the finished values of the application.

### Benefits
- The service is thin.

### Costs
- Business rules live in infrastructure; replacing HTTP by gRPC rewrites them, and they are tested through an emulator of the transport.

## Service over two clients when the data moves to another API

### Description
When the provider moves the name to another API, the user service wraps both clients and keeps its interface.

### Benefits
- No caller of the service changes.

### Costs
- The service joins two systems, which is business logic; replacing one transport edits the service holding the other's logic.
- The application's new dependency on a second system is hidden in one service instead of being decided at the level of the project.
