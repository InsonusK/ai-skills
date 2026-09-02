---
description: Extend the base workspace with bundle-size budgets enforced per project, and the route-data convention used by the custom preloading strategy
element_kind: repository
change_kind: extend
tags:
  - solution/lazy-loading-routing
  - element/repository
---

# Structure

No new directories. This extension adds two conventions on top of [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create]] and [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/Repository.extend]]:
- Every `type:app` and routable `type:feature` project declares a `budgets` block in its build configuration.
- Any route that should be background-preloaded carries `data: { preload: true }`, set only by the project that mounts it (see Rules below).

# Rules

## MUST
- Every `type:app` project MUST declare an initial-bundle budget (`error` threshold, not just `warning`) in its build configuration, so an accidental non-lazy import that grows the initial bundle fails CI rather than merely warning.
- Every routable `type:feature` project MUST declare a per-chunk budget for its own lazy chunk.
- The `data: { preload: true }` flag MUST be set only at the mounting point (the shell's `app.routes.ts` for top-level segments, or a module's own routes for the features it contains) — never inside the feature's/module's own exported routes.

- Never a feature or embeddable module MUST NOT set `preload: true` on its own routes to opt itself into preloading — that decision belongs to whoever mounts it (see `solution-app-routing`'s hierarchical ownership principle).
## SHOULD
- Bundle budget thresholds SHOULD be reviewed and adjusted deliberately when a feature's legitimate size grows, rather than silenced by raising the threshold reflexively.

- **Raising a bundle budget threshold to make a CI failure go away without investigating the cause** — Consequence: defeats the purpose of the budget — a genuine regression (e.g. a non-lazy import accidentally pulling a whole feature into the initial bundle) goes unnoticed — Instead: investigate why the bundle grew; only raise the threshold if the growth is a deliberate, reviewed trade-off
- **A feature marking its own route `preload: true` "because it's important"** — Consequence: bypasses the mounting point's authority over the preloading decision, and can silently preload an expensive federated remote chunk the shell never intended to warm up — Instead: the feature/module stays silent on preloading; the shell or parent module decides and sets the flag when mounting it
# Unittest TestCases

- [ ] WHEN a non-lazy import accidentally pulls feature code into the initial bundle THEN
  - [ ] the `type:app` project's initial-bundle budget fails the build with an error, not a warning
- [ ] WHEN a feature's own exported `Routes` are inspected THEN
  - [ ] none of them set `data: { preload: true }` on themselves
