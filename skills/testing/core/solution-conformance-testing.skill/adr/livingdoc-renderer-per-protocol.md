---
name: livingdoc-renderer-per-protocol
description: Decide how every stack produces the same filterable living-doc HTML report from its Cucumber runner.
problem: Each stack's runner speaks one of two report protocols (classic Cucumber JSON or Cucumber Messages); without a shared rendering step every stack builds its own HTML, or none.
decision: One pinned renderer per protocol, installed once in an isolated tools/livingdoc/ npm package that is identical in every stack — multiple-cucumber-html-reporter for classic JSON, @cucumber/html-formatter for Messages.
tags:
  - solution/conformance-testing
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
`public/scenarios/` lists the inventory but is a plain table. Several ecosystems already produce a polished, filterable living-doc view from the runner's report. The runners split into two protocols — classic Cucumber JSON (godog, Cucumber-JVM, cucumber-ruby, pytest-bdd, behave via plugin) and Cucumber Messages (cucumber-js, Reqnroll) — and each protocol has a well-known Node renderer. Open points were: who pins the renderer version, which renderer handles Messages, and where the Node install lives in a project whose primary language is already Node.

# Selected variant
**Selected variant:** [[#Shared pinned renderers in tools/livingdoc]]
- Chosen by the project owner on all three points: one pin in the base, the official Messages renderer, an isolated install in every stack.

# Searched variants

## Shared pinned renderers in tools/livingdoc

**Selected.**

### Description
`solution-conformance-testing` owns `tools/livingdoc/package.json` (exact versions) and `render.mjs`; every stack copies both verbatim and only makes its runner write the standard protocol to `tmp/report/tests/cucumber/`.

### Benefits
- The same input renders the same page in every stack; one place to bump a version.
- Renderer dependencies never enter the application's own manifest, TypeScript included.
- Adding a stack needs no renderer work — only the runner's formatter setting.

### Costs
- Every stack needs Node 22+ in the devcontainer and CI; without it the step is skipped.
- A second `package-lock.json` to maintain per project.

## Per-stack renderer pins

### Description
The base names the packages; each `-in-{stack}` pins its own compatible versions.

### Benefits
- A stack can move ahead without waiting for the others.

### Costs
- Versions drift; the same protocol renders differently across projects.

## Per-stack renderer choice for Messages

### Description
Any renderer per stack (e.g. Reqnroll LivingDoc for .NET), as long as the protocol is documented.

### Benefits
- Uses each ecosystem's most familiar view.

### Costs
- No shared output shape; reintroduces one renderer per stack.

## Renderer in the project's own package.json for Node-based stacks

### Description
TypeScript adds the renderer as a devDependency; other stacks use `tools/livingdoc/`.

### Benefits
- One `npm ci` for a TypeScript project.

### Costs
- Two layouts to document; renderer deps leak into the app's dependency tree and audit surface.
