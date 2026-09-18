---
name: business-named-port-not-generic-cache
description: Whether the caching port is named for its business data or is a generic Cache[K,V] interface
problem: A cache-backed outbound port could be declared as one generic interface (Get/Set/Delete keyed by string, or a Go generic Cache[K,V]) reused by every caching need, or as one narrow interface per business concept the domain actually caches.
decision: One narrow, business-named port per cached concept, matching the reference implementation's own ChatStore shape.
tags:
  - solution/cached-db
  - concern/documentation
  - concern/documentation/adr
  - stack/go
---

# Problem

`solution-cached-db` needs to decide the shape of the port `internal/domain/services` depends on to read/write cached data. Two structurally different options exist: a generic cache abstraction (`Cache.Get(key string) ([]byte, bool)` or a Go-generic `Cache[K, V]`) that any domain concept could reuse, or a narrow interface named for the one business concept being cached (e.g. `ReputationCache`).

# Selected variant

**Selected variant:** [[#One narrow, business-named port per cached concept (selected)]]

# Searched variants

## One narrow, business-named port per cached concept (selected)

### Description

Declare a distinct interface per cached concept, e.g. `ReputationCache` with `Get(ctx, url) (Reputation, bool, error)` / `Set(ctx, url, Reputation) error` — typed in the domain's own terms, not `[]byte` or `any`.

### Benefits

- Matches this family's own reference implementation (`tmp/tg-bot-service`'s `ChatStore`) exactly — a business-named port for its Redis-backed chat/quiz mapping, not a generic cache interface.
- The port's type signature is the domain's own type, so no caller marshals/unmarshals at the call site — that responsibility stays inside the adapter, where the caching technology already lives.
- A cache miss is expressed in the domain's own vocabulary (a typed `ok bool` or zero value), not a generic "key not found" the caller has to interpret back into domain terms.

### Costs

- A module with several independently-cached concepts ends up with several small port interfaces instead of one shared one — more interface declarations, though each is short.

## A generic Cache[K, V] port

### Description

Declare one Go-generic interface, `Cache[K comparable, V any]`, with `Get(ctx, key K) (V, bool, error)` / `Set(ctx, key K, value V) error`, reused for every cached concept by instantiating it with different type parameters.

### Benefits

- One interface declaration to maintain regardless of how many concepts get cached.
- Enforces a consistent method shape across every use of caching in the module.

### Costs

- A generic interface parameterized by the adapter's own choices (TTL policy, serialization) either grows hidden per-instantiation configuration the interface itself cannot express, or pushes that configuration to construction time in a way the domain layer neither needs nor should care about.
- Contradicts this family's own established pattern (every other port in this catalog — `QuizAgent`/`ChatStore`/`Notifier` in the reference, `ReputationChecker` in `solution-external-integration` — is narrow and business-named), introducing one inconsistent shape for no benefit specific to caching.

## Bytes-in/bytes-out cache port (`Get(key string) ([]byte, bool)`)

### Description

One non-generic interface using `string` keys and `[]byte` values; callers marshal/unmarshal at the call site.

### Benefits

- Simplest possible interface; trivially reusable across concepts without Go generics.

### Costs

- Pushes serialization into `internal/domain/services`, which then has to import an encoding package purely to talk to its own cache — a domain-layer concern the port exists to keep out.
- Loses type safety at every call site; a typo in a struct field during manual (un)marshaling fails at runtime instead of compile time.
