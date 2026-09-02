---
description: Extend the base workspace conventions with the rule that every routable project (feature or embeddable module) exposes its routes through its public API, relative to its own root only
element_kind: repository
change_kind: extend
tags:
  - solution/app-routing
  - element/monolith-repository
---

# Structure

No new directories are introduced. This extension adds a convention on top of the existing `index.ts` public-API rule from [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create]]: any `type:feature` project that is routable exports a `Routes` array (its own root-relative routes) from `index.ts`, in addition to whatever components/store it already exports.

# Rules

## MUST
- Every routable `type:feature` project MUST export its `Routes` array from `index.ts`, using paths defined only relative to its own root (e.g. `page`, not `{feature}/page`).
- A project at any level (shell, embeddable module, feature) MUST NOT define a route path that reaches outside the root segment it owns.
- The project that mounts a child (shell mounting a module or feature; a module mounting its features) MUST assign the root segment (e.g. `feature1/`) at the mounting point — the child itself never declares its own mount prefix.

- a `type:app` project (the shell) must never reference a path that exists two or more levels below its own mount point (e.g. the shell must not hardcode `module1/feature1/page` — it only knows about `module1/`).
# Unittest TestCases

- [ ] WHEN a feature's exported `Routes` array is inspected THEN
  - [ ] no path in it includes the feature's own mount segment
- [ ] WHEN the shell's route config is inspected THEN
  - [ ] it only references first-level mount segments (module/feature roots), never a path nested two or more levels deep

## SHOULD
- **A feature exporting routes with its own name baked into the path (e.g. `orders/list` instead of `list`)** — Consequence: the feature silently assumes it will always be mounted at a specific segment, breaking the moment it is remounted elsewhere (e.g. under a different embeddable module) or renamed at the mount point — Instead: the feature only ever defines paths relative to its own root; the mounting project decides the segment name
- **The shell reaching into a feature's routes to override or add a path two levels down** — Consequence: breaks the hierarchical ownership this solution exists to enforce, and reintroduces exactly the coupling `@nx/enforce-module-boundaries` and `index.ts` barriers are meant to prevent — Instead: the shell only ever augments its own segment's children; changes inside a feature's route tree happen inside that feature
