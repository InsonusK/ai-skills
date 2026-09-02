---
name: caching-strategy-per-content-type
description: Which Workbox caching strategy applies to each distinct kind of request the application makes
problem: Different kinds of requests have different freshness/availability trade-offs — a single caching strategy applied uniformly would either serve stale data where correctness matters, or fail to serve anything where availability matters most
decision: Five distinct strategies by content type — precache for the app shell, cache-first for static design-system assets, stale-while-revalidate for API GET reads, network-only for auth/mutations, and stale-while-revalidate runtime caching for federated remote chunks
tags:
  - solution/offline-first
  - concern/documentation
  - concern/documentation/adr
---

# Problem

Not every request the application makes has the same tolerance for staleness or the same need for offline availability. The app shell must always load, even offline. Static assets rarely change and are safe to serve straight from cache. API reads benefit from instant display of last-known data while quietly refreshing. Auth and mutation requests must never be served from a cache — a cached login response or a cached mutation result would be actively wrong. Federated remote chunks (per `solution-platform-embeddability`'s Dynamic Federation) are only resolved at runtime, ruling out a build-time precache manifest for them. A single uniform strategy cannot satisfy all of these at once.

# Selected variant

**Selected variant:** [[#Five strategies by content type]]

- **App shell** (HTML/CSS/JS, including lazy-loaded feature chunks per the "Lazy loading routing" solution): **precache** at service worker install time, so the whole shell updates atomically as one unit.
- **Static design-system assets** (fonts, icons, images): **cache-first** — they change rarely, so cache hits are both the fast and the correct path.
- **API GET requests** (feature data reads, via each feature's Client per `solution-api-http-layer`): **stale-while-revalidate** — instant response from cache, with a background refresh for next time.
- **Auth endpoints and all non-GET requests** (login/refresh, and every POST/PUT/DELETE mutation): **network-only** — never read from or written to any cache.
- **Federated remote chunks** (`remoteEntry` and exposed modules, resolved at runtime via the platform's remote registry): **stale-while-revalidate via runtime caching** (matched by URL pattern as requests occur), not precache, since their URLs are unknown until the platform's runtime remote registry resolves them.

# Searched variants

## Five strategies by content type

### Description

See "Selected variant" above.

### Benefits

- Each content type gets the freshness/availability trade-off appropriate to it, instead of one compromise applied everywhere
- Precaching the app shell atomically avoids the failure mode of a partially-updated, internally-inconsistent set of shell files
- Stale-while-revalidate for API reads gives the offline-first experience this solution exists to provide (Scenario A: something is always shown, even if momentarily stale) without the latency cost of waiting on the network first
- network-only for auth/mutations is a hard safety requirement, not just a preference — it directly protects the token-handling rules from `solution-authentication` (a cached auth response would be a serious bug) and correctly reflects that mutations are out of this solution's scope (offline mutation handling belongs to the future `solution-offline-sync`)
- Runtime caching (rather than precaching) for federated remote chunks is the only strategy compatible with Dynamic Federation's runtime URL resolution, while still letting a previously-loaded embeddable app keep working from cache if that team's deployment is temporarily unreachable

### Costs

- Five separate routing rules to configure and keep correct, rather than one blanket policy
- Engineers adding a new kind of request (a new API endpoint, a new static asset type) need to know which of the five categories it falls into, rather than it being automatic

## Single uniform strategy (e.g. stale-while-revalidate everywhere)

### Description

Apply one caching strategy to every request, regardless of content type.

### Benefits

- Simplest possible configuration — one rule, no categorization needed
- Consistent mental model for every request

### Costs

- Applied to auth/mutation endpoints, this would be an active correctness and security bug — a cached login/refresh response served from cache is exactly the outcome `solution-authentication`'s token-handling rules exist to prevent
- Applied to the app shell, a non-atomic caching strategy risks serving a partially-updated shell (some files from the new version, some from the old), rather than the atomic precache-and-swap behavior an app shell needs
- Applied to federated remote chunks with a precache-oriented strategy, it simply wouldn't work, since those URLs aren't known at service worker install time

## No service worker caching for API/feature data — only precache the app shell

### Description

Only the app shell (HTML/CSS/JS) is cached; every API request always goes to the network, with no offline fallback for data.

### Benefits

- Simpler service worker configuration — no risk of ever serving stale feature data
- No risk of a caching bug affecting business data correctness

### Costs

- Directly undermines the goal of this solution — the app shell would load offline, but every feature screen would show nothing (or an error) instead of last-known data, which is the core capability "Scenario A" exists to provide
