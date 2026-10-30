---
name: client, not adapter
description: Which word the client skills use for the code that calls one API of another service
problem: The skills called the code that calls an API the infrastructure adapter, while its folders and types are named client (`infrastructure/client/`, `TodoApiClient`); which word do the skills use?
decision: The skills call it the client, the code of one API in `infrastructure/client/{api}`; the shared code beside it stays the fixed client code; the general word of app-layers for any infrastructure is not used for it.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The text of api-client and of the client variation points said adapter, the code said client, and the owner talks of client and service. A reader had to learn that two words name one thing. Owner decision, 2026-10-30: one word, client.

# Selected variant
[[#Client]]

# Searched variants

## Client

**Selected.**

### Description
The code that calls one API is the client; the service over it is the service; the shared code under `infrastructure/client/core/` is the fixed client code.

### Benefits
- The text uses the names of the folders and the types.
- One pair of words, client and service, in the text and in conversation.

### Costs
- The older catalogs built on `solution-integration-client-layering` used Client for what is now the service; a reader of both meets one word in two meanings until those catalogs are replaced.

## Infrastructure adapter

### Description
The general word of [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md) for anything in the infrastructure layer.

### Benefits
- One word for clients and stores.

### Costs
- The text and the code name the same thing differently.
