---
name: one scope per command
description: How a command gets its DbContext, which serves one call at a time - a DI scope per command opened by the sender, a scope opened by every entry, or a context factory
problem: Entry points take ISender from the root provider, so everything a command resolves lives as long as the process. A DbContext must live for one command, shared by its nested commands.
decision: ScopedSender replaces ISender in one line of the composition root; it opens a scope for a command sent from outside any scope and reuses it for nested commands, found through an AsyncLocal. Entry points do not change. ValidateScopes is on.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
A variation point must attach without editing the entry points, and nested commands must share one context, so one unit of work can hold them.

# Selected variant
[[#A sender that opens the scope]]
- Agreed with the owner in the catalog's backlog, built here.

# Searched variants

## A sender that opens the scope

**Selected.**

### Description
`AddCommandScope()` after the modules; `ScopedSender` is a singleton `ISender` that resolves `IMediator` from the command's scope.

### Benefits
- Every entry point, present and future, gets scopes for nothing; a handler's `ISender` joins its parent.

### Costs
- Streams are refused, since no scope could end with one.
- Commands run in parallel inside one handler would share a context; they run one at a time.

## A scope in every entry

### Description
Each endpoint creates a scope and takes `ISender` from it.

### Costs
- Every API variation point changes, and the next one must remember.

## A context factory

### Description
Stores stay singletons and create a context per call through `IDbContextFactory`; a transaction would travel in an `AsyncLocal`.

### Benefits
- No scope; the services stay singletons.

### Costs
- A hand-built counterpart of what scoped registration gives; every other per-command service would need the same.
