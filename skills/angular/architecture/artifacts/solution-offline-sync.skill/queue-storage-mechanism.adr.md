---
name: queue-storage-mechanism
description: What storage/reactivity layer backs the offline mutation queue
problem: Raw IndexedDB is too low-level to give reactive UI updates without hand-rolling a change-notification system; RxDB's replication model assumes the backend is a document store synced via whole-document push/pull, which does not match this application's command-oriented API (PUT/PATCH entities, but also distinct RPC-style operations like POST /tasks/{id}/set-complete)
decision: Use Dexie.js as the storage/reactivity layer, with the actual replay/retry/conflict orchestration built as custom code on top of the existing Facade/Client architecture from the "API/HTTP-слой" solution
---

# Problem

The offline mutation queue needs: durable, structured storage that survives a page reload; reactive queries so the UI can show "N actions pending sync" without manual event wiring; and a replay mechanism that calls this application's own Facade methods (business validation) and Client methods (specific backend endpoints — entity CRUD, but also distinct command endpoints like `POST /tasks/{id}/set-complete`) — not a generic document-sync protocol. We need to decide what sits between raw IndexedDB and the queue's own orchestration logic.

# Selected variant

**Selected variant:** [[#Dexie.js + custom orchestration on top of Facade/Client]]

Dexie.js is used purely as a typed, reactive storage layer for queue records — nothing more. The actual replay/retry/conflict logic is custom code built on top of the existing Facade/Client architecture, calling each queued command's originating Facade method (or the underlying Client operation) directly, exactly as it would be called online.

# Searched variants

## Dexie.js + custom orchestration on top of Facade/Client

### Description

Dexie.js provides typed tables, transactions, and `liveQuery()` for reactive UI updates as queue records are added/removed. It carries no opinion about what "sync" means — the queue simply stores records of the form "call this Facade operation with these arguments," and a custom replay orchestrator (triggered by the `connectivity` slice from the "Offline-first" solution) works through the queue, invoking the real Facade/Client methods already established by the "API/HTTP-слой" solution.

### Benefits

- Matches this application's actual API shape — command-style operations (`set-complete`, and similar business actions) are stored and replayed exactly as "call this specific method," with no attempt to force them into a generic document-sync model
- No dependency on a document-replication protocol this application was never designed around — the queue's replay logic is just calling existing Facade methods, code the team already understands
- `liveQuery()` gives reactive "pending sync" UI for free, without hand-rolling IndexedDB change notifications the way a raw `idb`-based approach would require
- Small, focused dependency (~29KB) rather than a full reactive database with a replication engine mostly unused by this design

### Costs

- The replay/retry/conflict orchestration has to be built by hand — Dexie provides no built-in sync engine, unlike RxDB
- No built-in conflict-resolution primitives — this solution's own conflict-handling logic (see [[./conflict-resolution-strategy.md]]) has to be designed and implemented from scratch, rather than configuring an existing one

## RxDB (full replication engine)

### Description

Adopt RxDB as both storage and synchronization engine, using its push/pull replication protocol to sync local documents with the backend.

### Benefits

- Built-in replication engine, reactive queries, schema validation, and conflict-resolution primitives out of the box
- Mature ecosystem with plugins for various sync protocols (CouchDB, GraphQL, custom)

### Costs

- RxDB's replication protocol is fundamentally document-oriented: it compares and syncs whole document states between a local collection and a backend treated as a document store. This application's backend instead exposes semantically specific operations (`PUT`/`PATCH` an entity, but also standalone commands like `POST /tasks/{id}/set-complete`) that don't naturally express as "here is the new document state, please persist it"
- Command-style operations would need custom push/pull handlers that bypass RxDB's own document-comparison conflict handling anyway, meaning the parts of RxDB this application would actually use shrink to "a reactive local store," while still carrying the weight and conceptual overhead of its full replication engine
- Forces the application's mutation model to be reshaped around RxDB's document-collection abstraction, rather than reusing the Facade/Client architecture already established and battle-tested by the "API/HTTP-слой" solution

## Raw IndexedDB (or a thin wrapper like idb)

### Description

Use the native IndexedDB API directly, or a minimal promise wrapper, with no query/reactivity layer.

### Benefits

- Zero additional dependency weight beyond what's already used elsewhere (e.g. the "Логирование" solution's retry queue)
- Full, low-level control

### Costs

- No built-in reactivity — a "pending sync" UI would require hand-building a change-notification system (e.g. `BroadcastChannel` or a manual observable wrapper), which Dexie's `liveQuery()` already provides
- More boilerplate for basic CRUD operations against the queue table, compared to Dexie's typed, chainable query API
