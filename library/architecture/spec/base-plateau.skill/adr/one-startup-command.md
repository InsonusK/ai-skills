---
name: one startup command
description: What the base plateau contains — how far it goes before any variation point is attached
problem: Every variation point attaches to the base plateau; what must the base plateau already contain so that a variation point is described alone, never together with another one?
decision: The base plateau is a program that starts, runs exactly one command, and exits; it has no entry point other than the startup command and no interface to anything outside the process — each variation point brings its own entry point or its own service interface.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The previous plateau catalogs described each combination of capabilities as a separate project, so a change to one capability rewrote several plateaus. The base plateau has to be the one fixed point every variation point attaches to; the question is what it holds before any variation point is attached.

# Selected variant
[[#One startup command, no external interfaces]]
- Owner decision, 2026-10-14: the base plateau cannot be called flexibly; at most it runs and executes one command on start.

# Searched variants

## One startup command, no external interfaces

**Selected.**

### Description
Domain, application (commands and handlers), one startup entry point, and a composition root. An API variation point adds an entry point that calls commands; a service variation point (a store, a client, TaskBox) adds a service and the interface a handler uses it through.

### Benefits
- The same base serves a service and a CLI application; neither carries a transport it does not use.
- A variation point's description holds its own interface, so the base never changes when a service variation point is added or reworked.
- The base runs and is tested with no infrastructure at all.

### Costs
- A handler gains a constructor parameter when it starts using a service; the variation point's example has to show that change.
- The sample domain is too small to show a persisted state until the store variation point is attached.

## Base with store interfaces and in-memory implementations

### Description
The base plateau declares the repository and unit-of-work interfaces with in-memory implementations; a store variation point only replaces the implementation.

### Benefits
- Handlers are written against the store interface from the start and do not change when a real store is attached.

### Costs
- The base fixes the shape of a store before any store exists, and that shape differs per stack (generic repository with specifications in dotnet, one interface per aggregate in Go).
- A CLI application that never stores anything carries store interfaces it does not use.

## Base with an HTTP API

### Description
The base plateau is an HTTP service; other entry points are variation points.

### Benefits
- Matches the most common service shape directly.

### Costs
- Every CLI application carries an HTTP server, or needs a second base.
- HTTP becomes a special entry point that the other API variation points cannot be described like.
