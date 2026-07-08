---
name: preloading-strategy
description: Choice of preloading strategy for lazily-loaded routes (features and embeddable modules)
problem: Once routes are lazy-loaded via loadChildren, should their chunks be preloaded in the background, and if so, which ones and when
decision: Use a custom selective preloading strategy driven by a route-data flag, not Angular's built-in PreloadAllModules or NoPreloading
---

# Problem

The "App routing (база)" solution makes every feature and embeddable module lazy via `loadChildren` — none of their code is in the initial bundle. Left as-is (Angular's default `NoPreloading`), a chunk is only fetched the moment the user navigates to it, which costs a visible delay on first navigation into every section. We need to decide whether idle time after initial load should be spent prefetching some of those chunks in the background, and if so, which ones — given that this workspace can contain many features and, per the platform-embeddability solution, remote chunks belonging to independently deployed embeddable apps.

# Selected variant

**Selected variant:** [[#Custom selective preloading via route data flag]]

Chunks are preloaded selectively, based on an explicit `data: { preload: true }` flag set on the owning route. This keeps preloading an opt-in decision made deliberately per feature/module, rather than an all-or-nothing default.

# Searched variants

## Custom selective preloading via route data flag

### Description

A custom `PreloadingStrategy` provider inspects each route's `data.preload` flag. Routes with `preload: true` are prefetched in the background after the initial navigation settles; everything else stays purely on-demand. The flag is set at the mounting point (shell's `app.routes.ts` for top-level segments, a module's own routes for its features) — i.e. by whoever owns the decision of "this section is common enough to warm up," not by the feature/module itself.

### Benefits

- Deliberate, auditable list of what gets preloaded — no chunk is warmed up "by accident"
- Scales safely to many features and to federated embeddable modules: a rarely-used feature or a remote module that is expensive to fetch is not preloaded unless explicitly marked
- The decision lives at the mounting point, consistent with the hierarchical route-ownership principle from the "App routing (база)" solution — a feature does not decide for itself whether it's important enough to preload; whoever mounts it does
- Avoids prematurely fetching an embeddable module's remote chunk from another team's deploy target before it's actually needed

### Costs

- Requires writing and maintaining a small custom `PreloadingStrategy`, instead of using a built-in one
- Preloading is only as good as the judgment behind each `preload: true` flag — an unmarked, frequently-visited feature gets no benefit until someone notices and flags it

## PreloadAllModules (Angular built-in)

### Description

Angular's built-in strategy that preloads every lazy-loaded route's chunk in the background, unconditionally, right after the initial navigation.

### Benefits

- Zero configuration — works out of the box
- Every section feels instant on first navigation once preloading completes

### Costs

- Preloads federated embeddable modules' remote chunks unconditionally, fetching code from other teams' independently deployed targets before the user ever asks for it — wasteful and, at scale, a real bandwidth/cost concern
- Does not scale as the number of features grows — a workspace with dozens of features would background-fetch all of them regardless of actual usage
- No way to deprioritize a rarely-used or expensive feature without opting the whole application out of preloading

## NoPreloading (Angular default)

### Description

Angular's default behavior: a lazy chunk is fetched only at the moment of navigation into it, nothing is preloaded.

### Benefits

- Simplest possible behavior, no extra code
- Minimum possible network usage — nothing is fetched that isn't immediately needed

### Costs

- Every first navigation into any feature or embeddable module pays a visible network round-trip, even for sections used on nearly every session
- No mechanism to trade a little background bandwidth for a snappier common-path experience
