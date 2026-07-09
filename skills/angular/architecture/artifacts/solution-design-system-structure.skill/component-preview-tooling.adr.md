---
name: component-preview-tooling
description: How components are previewed and visually reviewed during development
problem: Storybook is the conventional choice for component preview/documentation, but prior hands-on experience with it produced significant friction; a plain Angular demo application is a workable, lower-friction alternative
decision: Use a self-built demo Angular application, not Storybook
---

# Problem

Developers need a way to render and visually review each design system component in isolation (and in combination) during development, independent of any consuming application. Storybook is the conventional tool for this in the wider frontend ecosystem, but it was previously attempted for Angular components and found to add significant friction relative to its benefit.

# Selected variant

**Selected variant:** [[#Self-built demo Angular application]]

A plain Angular application, part of this repository's workspace, renders each component with a set of example usages, serving the same practical preview purpose Storybook would, without Storybook's own configuration and integration surface.

# Searched variants

## Self-built demo Angular application

### Description

A standard Angular application (the second project in this repository's Angular CLI workspace) imports the library and renders a page per component (or per component family) with representative usage examples, states, and variants.

### Benefits

- No dedicated tool-specific configuration layer (webpack/Vite integration, addon ecosystem, Angular-specific Storybook quirks) to fight with — it's a plain Angular app, using exactly the same tooling (Angular CLI, the workspace's own build) as everything else in this repository
- Directly reflects prior real-world experience: Storybook was tried for this exact use case and produced meaningful friction, while a plain demo app is a known-workable pattern
- Easy for any Angular developer to contribute to immediately — no separate tool-specific mental model beyond "it's an Angular app with example pages"

### Costs

- Loses some of Storybook's built-in ecosystem features (automatic prop/args controls UI, built-in accessibility addon, visual regression tooling integrations) — any of these would need to be hand-built or a different tool adopted if the need arises later
- No built-in "stories as documentation" convention — the demo app's structure and thoroughness depends entirely on the team's own discipline, not a framework enforcing a consistent format per component

## Storybook

### Description

The conventional dedicated tool for isolated component development, preview, and documentation, with a story-per-state authoring convention and a broad addon ecosystem.

### Benefits

- Rich, purpose-built ecosystem (accessibility addon, visual regression integrations, auto-generated docs from component metadata)
- Familiar convention (stories) in the broader frontend industry, useful if design-system contributors come from outside this specific team

### Costs

- Already attempted for this exact use case and found to produce significant friction and configuration overhead rather than net benefit — this is a direct, first-hand cost specific to this team, not a hypothetical
- Adds a second build/tooling stack (Storybook's own Angular builder integration) alongside the workspace's own Angular CLI build, which is exactly the kind of overhead the "self-built demo app" alternative avoids
