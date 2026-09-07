---
name: queue-partitioning-and-ordering
description: How the mutation queue is ordered and partitioned when replaying after connectivity is restored
problem: A single global FIFO queue means one stuck or slow operation (e.g. a struggling geolocation-dependent feature) blocks every other feature's pending mutations from syncing; a fully per-entity queue solves this precisely but adds real complexity (tracking dependencies between entities, cross-feature entity relationships)
decision: Partition the queue by feature — FIFO ordering within each feature's own partition, replayed in parallel across features
tags:
  - solution/offline-sync
  - concern/documentation
  - concern/documentation/adr
---

# Problem

When connectivity is restored, queued mutations need to be replayed in some order. Business operations generally need to preserve order within a related sequence (e.g. "create order" must replay before "update that order's priority"), but unrelated operations across different features have no such dependency and should not block each other. A single global FIFO queue guarantees correct ordering everywhere but means a slow or repeatedly failing operation in one feature (the example raised: a struggling geolocation-dependent feature) stalls replay for every other feature's pending mutations too. A fully per-entity partitioned queue solves this with maximum precision, but requires tracking dependencies between entities — including, potentially, entities in different features — which is a meaningfully harder problem to get right.

# Selected variant

**Selected variant:** [[#Partition by feature]]

The queue is partitioned by feature (matching the `scope:*` tag from `solution-repository-structure`'s Nx taxonomy). Within a single feature's partition, mutations replay strictly FIFO — preserving intra-feature ordering (e.g. create-then-update on the same entity, which normally happens within one feature). Different features' partitions replay independently and in parallel, so a stuck or slow partition does not block the others.

# Searched variants

## Partition by feature

### Description

Each feature (`libs/{feature}/data-access`) gets its own FIFO queue partition, identified by the feature's `scope` tag. The replay orchestrator processes all partitions concurrently; within a partition, it processes entries strictly in enqueue order, stopping that partition's replay on a failure (same "stop on first failure per cycle" pattern as the retry queue in `solution-logging-global`) without affecting other partitions.

### Benefits

- Directly solves the reported problem: a struggling feature (e.g. one dependent on a flaky geolocation service) only stalls its own partition, not the whole application's sync
- Preserves ordering guarantees for the common case — most command sequences that depend on each other (create-then-update on the same entity) happen within a single feature, which is exactly what feature-level partitioning preserves
- Meaningfully simpler to implement and reason about than per-entity partitioning — no dependency graph between individual entities needs to be tracked or resolved
- Aligns with the existing `scope:*` tag taxonomy already used throughout the architecture, rather than inventing a new partitioning dimension

### Costs

- Does not protect against a dependency between two entities in *different* features (e.g. a feature's mutation depends on another feature's queued-but-not-yet-synced entity) — if and when such cross-feature dependencies become common, a future solution may need finer-grained (per-entity, or explicit dependency-declaring) partitioning, as already anticipated
- Two mutations in the same feature that are actually independent of each other still serialize behind one another within that feature's partition, even though they could in principle replay in parallel

## Global FIFO (single queue)

### Description

One queue, one strict replay order across the entire application, regardless of feature.

### Benefits

- Simplest possible ordering guarantee — trivially correct for any cross-feature dependency, since everything replays in the exact order it was enqueued
- No partitioning logic to implement at all

### Costs

- Exactly the problem this ADR was raised to solve: one slow or repeatedly-failing operation (in any feature) blocks replay for every other feature's queued mutations, even entirely unrelated ones
- Does not scale well as the number of features and mutation types grows — the likelihood of some feature's queue getting stuck rises with scale, and its blast radius under global FIFO is always "the whole application's sync"

## Per-entity partitioning

### Description

Each individual entity (e.g. a specific order, a specific task) gets its own FIFO sub-queue; mutations targeting different entities replay fully in parallel, regardless of feature.

### Benefits

- Maximum parallelism and the most precise blast radius — only mutations targeting the same specific entity ever block each other
- Naturally correct even for cross-feature dependencies, as long as they're expressed as dependencies on a specific entity

### Costs

- Requires tracking which entity each queued mutation targets, including entities referenced across different features — a real design problem (how is a cross-feature entity relationship declared and honored?) that this application does not yet have a clear model for
- Meaningfully more complex to implement and debug than feature-level partitioning, for a benefit (finer-grained parallelism than feature-level already provides) that is not yet demonstrated to be necessary
- The user's own stated concern (one stuck feature blocking others) is already fully addressed by feature-level partitioning — per-entity partitioning would be solving a problem beyond what's currently known to exist, at meaningfully higher implementation cost
