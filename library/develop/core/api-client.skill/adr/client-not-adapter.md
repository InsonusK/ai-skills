---
name: client, not adapter
description: Which word the client skills use for code that calls external API operations
problem: The skills called the code that calls an API the infrastructure adapter, while its folders and types are named client (`client/`, `TodoApiClient`); which word do the skills use?
decision: The skills call API-calling code the client, under `client/{name}`; its shared mechanics are fixed client code under `client/core/`; Client remains distinct from the general infrastructure layer.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

Amended on 2026-11-26: the name Client remains; the current sibling location and entity-focused scope follow [the sibling boundary decision](../../app-layers.skill/adr/sibling-client-and-system-service-boundaries.md) and [entity-focused grouping](./entity-clients-and-system-services.md).

# Problem
The text of api-client and of the client variation points said adapter, the code said client, and the owner talks of client and service. A reader had to learn that two words name one thing. Owner decision, 2026-10-30: one word, client.

# Selected variant
[Client](#client)

# Searched variants

## Client

**Selected.**

### Description
The API-calling code under `client/{name}/` is the client; the service over it is the service; shared mechanics under `client/core/` are the fixed client code. Client is a sibling of infrastructure, not a directory inside it.

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
