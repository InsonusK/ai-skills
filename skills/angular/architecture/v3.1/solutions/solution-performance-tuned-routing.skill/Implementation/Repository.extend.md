---
description: Extend the base workspace with bundle-size budgets enforced per project, and the route-data convention used by the custom preloading strategy
element_kind: repository
change_kind: extend
tags:
  - solution/performance-tuned-routing
  - element/monolith-repository
---

# Structure

No new directories. This extension adds two conventions on top of [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create]] and [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/Repository.extend]]:
- Every `type:app` and routable `type:feature` project declares a `budgets` block in its build configuration.
- Any route that should be background-preloaded carries `data: { preload: true }`, set only by the project that mounts it (see Rules below).

# Rules

## MUST
- Every `type:app` project declares an initial-bundle budget with an `error` threshold (not just `warning`).
  - Risk: an accidental non-lazy `import` of a feature grows the initial bundle and only *warns* — it ships.
  - Fix: `budgets` in `project.json` with `type: initial` and `maximumError`; CI fails the build.
- Every routable `type:feature` project declares a per-chunk budget for its own lazy chunk.
  - Risk: a feature quietly bloats its chunk (a heavy chart lib, a PDF renderer) with no signal until users feel the load time.
  - Fix: a `bundle`/`anyScript` budget scoped to the feature's chunk name.
- `data: { preload: true }` is set only at the mounting point (the shell's `app.routes.ts`, or a module's own routes for the features it contains) — never inside a feature's/module's own exported routes.
  - Risk: a feature preloading itself preloads for every host, defeating the host's per-context choice.
  - Fix: the mounting project decides; the feature's routes stay silent on preloading.
- A feature or embeddable module never sets `preload: true` on its own routes to opt itself into preloading.
  - Risk: same as above — the mounted-vs-mounting ownership from `solution-app-routing` is violated.
  - Fix: preloading is the mounting point's call; a feature that wants it says so in a comment, not in `data`.
## SHOULD
- Bundle budget thresholds should be reviewed and adjusted deliberately when a feature's legitimate size grows, rather than silenced by raising the threshold reflexively.

- **Raising a bundle budget threshold to make a CI failure go away without investigating the cause** — Consequence: defeats the purpose of the budget — a genuine regression (e.g. a non-lazy import accidentally pulling a whole feature into the initial bundle) goes unnoticed — Instead: investigate why the bundle grew; only raise the threshold if the growth is a deliberate, reviewed trade-off
- **A feature marking its own route `preload: true` "because it's important"** — Consequence: bypasses the mounting point's authority over the preloading decision, and can silently preload an expensive federated remote chunk the shell never intended to warm up — Instead: the feature/module stays silent on preloading; the shell or parent module decides and sets the flag when mounting it
# Unittest TestCases

- [ ] WHEN a non-lazy import accidentally pulls feature code into the initial bundle THEN
  - [ ] the `type:app` project's initial-bundle budget fails the build with an error, not a warning
- [ ] WHEN a feature's own exported `Routes` are inspected THEN
  - [ ] none of them set `data: { preload: true }` on themselves
