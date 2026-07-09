---
name: embedding-mechanism
description: Choice of the runtime mechanism by which independently built and deployed applications are embedded into the platform shell
problem: How should independently deployed applications be loaded into and integrated with the platform at runtime, given the need for independent deploy/versioning per team and shared state/event exchange with the host
decision: Use Native Federation with Dynamic Federation
---

# Problem

Embeddable applications are built and deployed by separate teams/repositories, on their own release cycles, yet must exchange state and events with the platform shell at runtime and feel like part of one integrated product. We need a loading/integration mechanism that:
- allows each embeddable app to be built and deployed independently of the platform and of other embeddable apps;
- lets the platform discover and load an embeddable app without being rebuilt when that app ships a new version;
- allows real, low-friction state/event exchange between host and embedded app, not just one-way rendering.

# Selected variant

**Selected variant:** [[#Native Federation + Dynamic Federation]]

Native Federation is selected because it matches Angular's current default build system (the esbuild-based `ApplicationBuilder`), is positioned by its maintainers and the Angular team as the direct, API-compatible successor to Webpack Module Federation, and — combined with Dynamic Federation — lets the platform resolve an embeddable app's URL at runtime instead of at build time. Both host and remote run in the same JavaScript runtime and the same Angular instance (shared as a singleton dependency), so state/event exchange can be a real shared service rather than serialized cross-context messaging.

# Searched variants

## Native Federation + Dynamic Federation

### Description

Each embeddable app is built as a federation "remote", exposing an entry point (a component or bootstrap function) via a `remoteEntry` manifest. The platform shell acts as a "dynamic host": it reads the list of available remotes and their URLs from a runtime configuration (not baked in at build time — "build once, deploy everywhere"), and lazily loads a remote's code directly into its own running Angular application. Angular itself, plus any library both sides mark as `singleton: true` (e.g. a shared contracts package), is loaded once and reused by both host and remote.

### Benefits

- Matches Angular's current default build pipeline (esbuild `ApplicationBuilder`); does not require pinning the workspace to the legacy webpack builder
- Host and remote share one Angular runtime — real object-reference state sharing is possible through a singleton shared service, not just message passing
- Dynamic Federation lets the platform pick up a new version of an embeddable app deployed to its existing URL without a platform rebuild or redeploy
- API-compatible with Webpack Module Federation, so tooling/knowledge transfers and migration in either direction stays cheap if ever needed
- Nx has first-class generators for host/remote setup and dependency-graph awareness of federation boundaries

### Costs

- Host and remotes must agree on a shared-dependency contract (Angular version range, shared contracts package version) — a remote built against an incompatible major version can duplicate the Angular runtime or fail to load, depending on `strictVersion` configuration
- Slightly more moving parts at deploy time than a single monolithic build (remote manifests, runtime resolution of URLs)
- Debugging cross-remote issues (e.g. a shared singleton getting duplicated) requires understanding the federation runtime, which is an extra concept for the team

## Webpack Module Federation

### Description

The original implementation of the same mental model, shipped with webpack 5. Functionally very similar to Native Federation (host/remote, shared singleton dependencies, dynamic remote resolution), but requires the Angular CLI's webpack-based builder rather than the modern esbuild `ApplicationBuilder`.

### Benefits

- Mature, widely documented, large existing body of tutorials and production usage
- Identical mental model to Native Federation (host/remote/shared), so the architectural rules of this solution would not change if migrating between the two

### Costs

- Requires stepping away from Angular's current default esbuild-based build pipeline, giving up its build-performance improvements
- The tooling maintainers themselves are steering new projects toward Native Federation as the forward-compatible choice
- No practical advantage over Native Federation for this use case, given Angular's build system direction

## Web Components / Angular Elements

### Description

Each embeddable app is compiled into one or more custom elements (via Angular Elements) and loaded into the platform as a plain DOM element (e.g. via a `<script>` tag pointing at the remote's bundle).

### Benefits

- Framework-agnostic on the consuming side — the platform does not need to be Angular to host the element
- Strong encapsulation via Shadow DOM; styling isolation is easier to guarantee by default

### Costs

- Each embedded app typically ships its own Angular runtime and zone.js instance — no automatic singleton sharing, so multiple Angular copies can load on one page
- Real-time shared state/event exchange has no built-in mechanism; it must be hand-built on top of DOM events or a global `window` object, which is less structured and easier to get wrong than a shared singleton service
- Does not by itself give "build once, deploy everywhere" — the platform still needs its own mechanism to discover and register available elements at runtime

## iframe

### Description

Each embeddable app runs in its own `<iframe>`, fully isolated from the host page.

### Benefits

- Maximum isolation: a crash, memory leak, or global-scope pollution in an embedded app cannot affect the host or other embedded apps
- Simplest possible deployment story — an iframe just points at a URL, no shared build tooling or dependency versioning to coordinate

### Costs

- All communication must go through `postMessage`, which means every piece of shared state or event has to be explicitly serialized and passed across the boundary — directly conflicts with the requirement for low-friction runtime state/event sharing
- Styling and layout integration (matching the host's theme, resizing to content, shared modals/overlays) is significantly harder across an iframe boundary
- No shared Angular runtime — each iframe pays the full cost of bootstrapping its own Angular application
