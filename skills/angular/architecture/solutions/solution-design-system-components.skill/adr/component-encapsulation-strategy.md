---
name: component-encapsulation-strategy
description: How design system components relate to the Angular Material components they may be built on top of
problem: Application developers should never need to know Angular Material's own API or category model to use a design system component, and should be insulated from Material's own breaking changes across versions; a thin passthrough wrapper does not achieve this
decision: Full API encapsulation — every design system component has its own selector and its own, independently designed API (inputs/outputs organized around this application's actual usage axes, not Material's categorization). Internally, a component may delegate to Angular Material's implementation, or be fully custom-built, decided per component based on whether Material's own implementation satisfies the real functional/performance requirements.
---

# Problem

Two principles drive this decision: design system components must be convenient for application developers to use, and application developers must not need to know anything about Angular Material's own API or how it changes across versions. A thin wrapper — one that mostly passes attributes through to an underlying Material component with a similar API shape — fails the second principle: consumers still end up thinking in Material's own terms (its color input, its category of button appearances), and a Material API change still likely means a design-system API change too. Separately, some components need behavior or performance characteristics Material's own implementation doesn't provide at all.

# Selected variant

**Selected variant:** [[#Full API encapsulation, internal implementation decided per component]]

Every design system component has its own selector (`ds-*`) and an API designed from this application's actual usage needs — not mirrored from whatever axes Material happens to expose. For example, a button component might organize its API around `variant` (solid/text/outline/fab), `size` (sm/md/lg), and `color`, plus composite fields like `action`/`dropdown` that determine whether the rendered result is a plain button, a dropdown, or a split button combining both — none of which corresponds directly to how Angular Material itself categorizes its button components. Internally, the component's template may use Angular Material's own components, or may be a fully custom implementation — this is decided per component, based on whether Material's own implementation meets the real requirements (feature completeness, performance, accessibility) for that specific case. A complex, performance-sensitive component (e.g. a tree with large datasets) may warrant a fully custom internal implementation even when Angular Material offers a superficially similar component.

# Searched variants

## Full API encapsulation, internal implementation decided per component

### Description

See "Selected variant" above.

### Benefits

- Application developers only ever learn this design system's own API — never Angular Material's inputs, category names, or component selectors directly
- A future Angular Material version bump can change or even remove an internally-used Material component's API with zero impact on application developers, as long as the design system's own component still fulfills its documented contract — the encapsulation boundary absorbs the change
- The API can be organized around concepts that actually matter for this application (e.g. "is this a button, a dropdown, or both" as a single coherent axis) rather than however Material happened to split up its own components
- Frees each component's internal implementation to use whatever approach best satisfies real requirements — delegating to Material where it's sufficient, building custom where it isn't (e.g. a large-dataset tree needing different performance characteristics than Material's own tree component provides)

### Costs

- Meaningfully more design and implementation work per component than a thin passthrough wrapper — the API has to be actually designed, not just mirrored
- The design system takes on full responsibility for its own API's stability and completeness — it can no longer lean on "whatever Material exposes" as an implicit contract, and must deliberately decide what to expose
- When Material does add a genuinely useful new capability, exposing it through the design system's own API requires a deliberate addition, rather than it becoming automatically available through a passthrough

## Thin wrapper (passthrough attributes, API close to Material's own)

### Description

The design system component's selector mostly forwards inputs/outputs to an underlying Material component with a similar shape (e.g. a `ds-button` accepting a `color` input that maps roughly 1:1 to Material's own `color` input).

### Benefits

- Much less design/implementation work per component — mostly a thin pass-through
- New Material capabilities are often trivially exposed with a small API addition, since the shape already mirrors Material's own

### Costs

- Application developers still end up thinking in Material's own terms and categorization, directly violating the "developers shouldn't need to know Material's API" principle
- A Material API change (a renamed input, a restructured category of appearances) is likely to force a corresponding design-system API change, propagating the churn straight through to every consuming application
- Does not accommodate cases where the application's real usage pattern doesn't map cleanly onto Material's own categorization (the button variant/action/dropdown example) — a thin wrapper can only expose what Material already models, not redesign it

## No wrapping — expose Angular Material components directly to consumers

### Description

Application code imports and uses Angular Material components directly; the design system provides only theming (tokens) and no component layer at all.

### Benefits

- Zero component-authoring or maintenance burden for this design system
- Full, immediate access to every Material capability, with no encapsulation lag

### Costs

- Every application developer needs to know Angular Material's own API in full, directly contradicting this solution's core principle
- No insulation whatsoever from Material's own breaking changes — every consuming application absorbs them directly
- No way to express the application's own composite concepts (like the button's action/dropdown axis) that don't correspond to any single Material component
