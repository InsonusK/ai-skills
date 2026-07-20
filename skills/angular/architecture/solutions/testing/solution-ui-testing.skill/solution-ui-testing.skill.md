---
name: solution-ui-testing
description: Behavioral component tests (Testing Library, reused from solution-app-testing's tooling ADRs) plus two layers no prior solution covers — Playwright screenshot regression and @axe-core/playwright accessibility scans, run against each plateau's own demo/preview pages, without Storybook or Chromatic
domain: skill
type: architecture
version: 1
tags:
  - skill/architecture/solution
  - angular
  - testing
  - ui
  - accessibility
  - visual-regression
triggers:
  - Writing a new UI component (platform feature component, form component, or design-system component)
  - Deciding whether a component needs business-layer mocking in its test
  - Reviewing whether a visual or accessibility regression could have been caught automatically
creates:
  - apps/component-preview (platform plateau only)
extends:
  - libs/{feature}/feature (component spec files under spec/, platform plateau)
  - libs/shared/ui (component spec files under spec/, platform plateau)
  - projects/design-system (component spec files under spec/, design-system plateau)
  - projects/demo (visual/a11y spec target via spec/preview/, design-system plateau)
depends_on:
  - "[[skills/angular/architecture/solutions/testing/solution-app-testing.skill/solution-app-testing.skill|App testing]]"
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|Структура репозитория (база)]]"
  - "[[skills/angular/architecture/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|Design system: структура]]"
  - "[[skills/angular/architecture/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|Design system: компоненты]]"
  - "[[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill.md|Формы]]"
adr:
  - "[[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/adr/visual-regression-approach|Visual regression approach ADR]]"
  - "[[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/adr/accessibility-testing-approach|Accessibility testing approach ADR]]"
  - "[[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/adr/style-snapshot-approach|Style snapshot approach ADR]]"
---

# Goal

- Test a UI component (including a form component, per [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill.md|Формы]]) purely on its own `input()`/`output()`/`model()` surface — independent of whatever business logic, if any, happens to sit around it in a real application
- Cover the visual and accessibility regressions that a behavioral component test structurally cannot detect (no layout engine in jsdom/happy-dom, no WCAG rule evaluation in a DOM-structure assertion)
- Apply the exact same approach, tooling, and rules to two distinct plateaus that live in two different repositories with two different workspace tools — the platform's own presentational/form components, and the design system's `ds-*` components
- Reuse this architecture's already-adopted tools (Vitest, Playwright, Testing Library) rather than introducing a new one

# Capabilities

- Fast, ESM-native [behavioral component tests](skills/angular/architecture/solutions/testing/solution-ui-testing.skill/glossary/behavioral-component-testing.md) via Vitest + Testing Library, verifying rendered behavior without ever needing business-layer mocks
- Automated [visual regression](skills/angular/architecture/solutions/testing/solution-ui-testing.skill/glossary/visual-regression-testing.md) coverage (Playwright screenshots, light and dark) for every component with a demo/preview page
- [Computed-style snapshot](skills/angular/architecture/solutions/testing/solution-ui-testing.skill/glossary/style-snapshot-testing.md) coverage (`getComputedStyle()` on a fixed, shared property list — not CSS class names) paired with every visual spec, turning a failing pixel diff into a named property/value change instead of an unexplained image difference
- Automated [accessibility](skills/angular/architecture/solutions/testing/solution-ui-testing.skill/glossary/accessibility-testing.md) regression coverage (`@axe-core/playwright`) for the same components, catching WCAG-checkable violations no human review step guarantees to catch
- One consistent approach across both the platform monorepo and the independently-repositoried design system — the only thing that differs between them is where the demo/preview pages physically live

# Core Principles

- A UI component is tested independently of the business logic around it: a signal/input on the way in, a rendered result/event on the way out — the test never needs to know what Facade, Client, or backend (if any) exists beyond the component's own immediate collaborator
- Two independent layers of UI testing exist, and neither substitutes for the other: **behavioral** (Testing Library — reused unchanged from the prior `solution-testing`, now scoped here) catches what a test author explicitly anticipated; **visual + accessibility** (Playwright screenshots + axe-core) catches what nobody anticipated — a broken layout, a silently-failing dark-mode branch, lost contrast, a specificity conflict, or a WCAG violation
- A pixel diff says *that* rendering changed; a computed-style snapshot says *why* — every `.visual.spec.ts` ships with a paired `.style-snapshot.spec.ts` (per [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/adr/style-snapshot-approach|style-snapshot-approach ADR]]), and a failing visual spec's baseline is never updated without checking the paired style-snapshot diff first
- Visual and accessibility checks run directly against each plateau's own demo/preview pages via Playwright — no Storybook, no Chromatic (see both ADRs for why)
- Both plateaus (platform, design system) apply the identical approach to all three layers; only the physical location of the demo/preview pages differs — `apps/component-preview` for the platform (a new, minimal harness this solution introduces), `projects/demo` for the design system (already existing, extended here)

# Adr

- [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/adr/visual-regression-approach|Playwright screenshot testing against demo/preview pages instead of Storybook + Chromatic]]
  - Selected variant: Playwright `toHaveScreenshot()` against existing demo/preview pages — reuses the already-adopted e2e tool, avoids re-introducing Storybook (already rejected for this team), and avoids a paid external SaaS
- [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/adr/accessibility-testing-approach|@axe-core/playwright against demo/preview pages]]
  - Selected variant: automated axe-core scans reusing the same Playwright pages the visual specs already navigate to
- [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/adr/style-snapshot-approach|Computed-style JSON snapshot per element, paired with the pixel screenshot]]
  - Selected variant: a fixed, shared list of `getComputedStyle()` properties (not CSS class names) snapshotted per component state, so a failing visual spec's diff is explained instead of just observed

# Requirements

SOLUTION:
- [[skills/angular/architecture/solutions/testing/solution-app-testing.skill/solution-app-testing.skill|App testing]]
  - Reuses its Vitest ADR ([[skills/angular/architecture/solutions/testing/solution-app-testing.skill/adr/test-runner-choice]]) and Playwright ADR ([[skills/angular/architecture/solutions/testing/solution-app-testing.skill/adr/e2e-framework-choice]]) without re-arguing the tool choice — this solution only extends how those same tools are applied to UI/visual concerns
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|Структура репозитория (база)]]
  - Platform plateau: the base Nx workspace `apps/component-preview` is added to
- [[skills/angular/architecture/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|Design system: структура]]
  - Design-system plateau: `projects/demo`, already established there, is reused and extended as the visual/a11y target — see [[skills/angular/architecture/solutions/solution-design-system-structure.skill/adr/component-preview-tooling|component-preview-tooling ADR]], which this solution's own visual-regression ADR builds directly on
- [[skills/angular/architecture/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|Design system: компоненты]]
  - The `ds-*` component authoring convention (signal-based API, full Material encapsulation) is exactly what gets tested at all three layers here
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill.md|Формы]]
  - A form component is the clearest example of a component tested purely at the UI level: its Signal Forms `FieldTree` state is entirely internal, and a test only ever needs to interact with the rendered form and assert on it — never on business logic around it

NPM:
- @testing-library/angular, @testing-library/user-event
  - DOM-level component testing utilities (reused from `solution-app-testing`'s dependency list — this solution is the actual consumer)
- @axe-core/playwright
  - Automated accessibility scanning inside a Playwright page

# Template Skill Mutations

PROJECT:
- [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/Implementation/PlatformComponents/component-preview.project.create|apps/component-preview]] - create - platform plateau's minimal harness app for rendering components in isolation
- [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/Implementation/DesignSystemComponents/demo.project.extend|projects/demo]] - extend - design-system plateau's existing demo app, now also the visual/a11y target

Artifact-level (generic patterns, applied identically to both plateaus):
- [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create|{component-name}.component.spec.ts]] - create - behavioral component test via Testing Library, faking only the component's own immediate dependency (if any)
- [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/Implementation/Testing/{component-name}.visual.spec.ts.create|{component-name}.visual.spec.ts]] - create - Playwright screenshot-regression spec against the component's demo/preview page
- [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/Implementation/Testing/{component-name}.style-snapshot.spec.ts.create|{component-name}.style-snapshot.spec.ts]] - create - computed-CSS-property snapshot spec, paired with the visual spec, explaining what a failing pixel diff actually changed
- [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/Implementation/Testing/read-visual-style-properties.ts.create|read-visual-style-properties.ts]] - create - shared helper + curated property list read by every style-snapshot spec
- [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/Implementation/Testing/{component-name}.a11y.spec.ts.create|{component-name}.a11y.spec.ts]] - create - `@axe-core/playwright` scan against the same demo/preview page

# Directory layout

All test files and their assets live under a single `spec/` directory next to the component implementation files. This keeps `component.ts`, `component.html` and `component.scss` free from test-related clutter.

```text
libs/{feature}/feature/src/lib/{feature}/{component-name}/
  {component-name}.component.ts
  {component-name}.component.html
  {component-name}.component.scss
  spec/
    {component-name}.component.spec.ts
    {component-name}.visual.spec.ts
    {component-name}.style-snapshot.spec.ts
    {component-name}.a11y.spec.ts
    snapshot/
      {component-name}-default-light.png
      {component-name}-default-dark.png
      {component-name}-default-light.styles.txt
      {component-name}-default-dark.styles.txt
    preview/
      {component-name}.preview.ts
```

For the design-system plateau the project prefix differs (`projects/design-system/src/lib/{component-name}/`), but the `spec/` structure is identical. The `snapshot/` files are committed baselines. The `preview/` files are small harness components consumed by the preview app (`apps/component-preview` for platform, `projects/demo` for design system). The shared `readVisualStyleProperties` helper is stored in the project's test-support directory (e.g., `libs/{feature}/feature/testing/read-visual-style-properties.ts` or `projects/design-system/testing/read-visual-style-properties.ts`) and is not duplicated per component.

# Workflow

## Adding a new UI component, fully tested (happy path)

1. Build the component (per [[skills/angular/architecture/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|Design system: компоненты]] or [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill.md|Формы]], depending on plateau/kind) with a signal-based `input()`/`output()`/`model()` API.
2. Add a `spec/{component-name}.component.spec.ts` (Testing Library) covering rendered behavior for each meaningfully distinct state.
3. Add a preview harness for the component — create `spec/preview/{component-name}.preview.ts` and register it in the preview app (`apps/component-preview` for platform or `projects/demo` for design system), one route/section per state.
4. Add a `spec/{component-name}.visual.spec.ts` screenshotting each state (both color schemes, where applicable).
5. Add a paired `spec/{component-name}.style-snapshot.spec.ts` for the same states, via [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/Implementation/Testing/read-visual-style-properties.ts.create|read-visual-style-properties]].
6. Add a `spec/{component-name}.a11y.spec.ts` scanning the same states.

![Testing a new feature end-to-end (happy path)](skills/angular/architecture/solutions/testing/solution-app-testing.skill/diagrams/testing-a-new-feature-end-to-end-happy-path.mmd)

## A visual regression slips past behavioral tests (the gap this solution closes)

1. A CSS change unintentionally breaks a component's dark-mode branch — every `light-dark()` value now resolves incorrectly in dark scheme.
2. The component's behavioral spec (Testing Library, jsdom/happy-dom) still passes — the DOM structure, roles, and text content are all unchanged; jsdom has no layout/paint engine and cannot evaluate computed color values at all.
3. The component's `spec/{component-name}.visual.spec.ts` dark-scheme screenshot fails against its committed baseline, catching the regression before merge.
4. The paired `spec/{component-name}.style-snapshot.spec.ts` also fails, naming exactly which properties (e.g. `color`, `backgroundColor`) no longer resolve to their dark-mode values — confirming this is a real regression, not rendering noise.

## An agent blindly updates a screenshot baseline (the gap style-snapshot closes)

1. A `spec/{component-name}.visual.spec.ts` fails after an unrelated CSS refactor; the agent, unable to interpret the pixel diff, is tempted to just run `--update-snapshots` and move on.
2. Per [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/adr/style-snapshot-approach|style-snapshot-approach]], the agent checks the paired `spec/{component-name}.style-snapshot.spec.ts` diff first.
3. If the style-snapshot diff is empty, the pixel change is rendering noise (anti-aliasing/font hinting) — safe to accept. If it names a changed property (e.g. `padding: 8px → 12px`), that is a real, reviewable change the agent must confirm as intentional before updating either baseline.

## Reaching for Chromatic instead (rejected path, caught in review)

1. An engineer, missing built-in diff-review tooling, proposes adopting Storybook + Chromatic for visual regression.
2. This is flagged against [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/adr/visual-regression-approach#Storybook + Chromatic|the rejected variant in this solution's ADR]] — Chromatic requires Storybook as a mandatory foundation, and Storybook was already evaluated and rejected for this team (see [[skills/angular/architecture/solutions/solution-design-system-structure.skill/adr/component-preview-tooling]]).
3. Fix: use the existing demo/preview page and a Playwright screenshot spec instead — no new tool, no reversal of the prior Storybook decision.

# Rules

## MUST
- [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create#MUST|spec/{component-name}.component.spec.ts]]
- [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/Implementation/Testing/{component-name}.visual.spec.ts.create#MUST|spec/{component-name}.visual.spec.ts]]
- [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/Implementation/Testing/{component-name}.style-snapshot.spec.ts.create#MUST|spec/{component-name}.style-snapshot.spec.ts]]
- [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/Implementation/Testing/read-visual-style-properties.ts.create#MUST|read-visual-style-properties.ts]]
- [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/Implementation/Testing/{component-name}.a11y.spec.ts.create#MUST|spec/{component-name}.a11y.spec.ts]]
- [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/Implementation/PlatformComponents/component-preview.project.create#MUST|apps/component-preview]]
- [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/Implementation/DesignSystemComponents/demo.project.extend#MUST|projects/demo]]

## MUST NOT
- [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/Implementation/PlatformComponents/component-preview.project.create#MUST NOT|apps/component-preview]]

# Anti-patterns

- [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create|See spec/{component-name}.component.spec.ts.create.md]] — asserting against `fixture.componentInstance` instead of the rendered DOM; wiring a mock the component doesn't actually inject.
- [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/Implementation/Testing/{component-name}.visual.spec.ts.create|See spec/{component-name}.visual.spec.ts.create.md]] — updating a baseline screenshot without understanding why it changed; screenshotting non-deterministic content.
- [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/Implementation/Testing/{component-name}.style-snapshot.spec.ts.create|See spec/{component-name}.style-snapshot.spec.ts.create.md]] — running `--update-snapshots` on a failing visual spec without checking the paired style-snapshot diff first; defining a component-specific property list instead of using the shared helper.
- [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/Implementation/Testing/{component-name}.a11y.spec.ts.create|See spec/{component-name}.a11y.spec.ts.create.md]] — disabling axe-core entirely instead of scoping an exception to one rule.
- [[skills/angular/architecture/solutions/testing/solution-ui-testing.skill/Implementation/PlatformComponents/component-preview.project.create|See component-preview.project.create.md]] — wiring a previewed component to a real Facade/Store/backend "to keep it realistic."
- **Reaching for `HttpTestingController` or a faked Facade/Client to test a component** — that concern belongs to [[skills/angular/architecture/solutions/testing/solution-app-testing.skill/solution-app-testing.skill|solution-app-testing]], not this solution; a component test never needs it.

# Check list

- [ ] Every UI component has a behavioral spec, a visual spec, a style-snapshot spec, and an a11y spec — the four layers ship together, not independently
- [ ] No component test fakes anything beyond the component's own immediate injected dependency
- [ ] Every demo/preview page state has a committed baseline screenshot, a committed style-snapshot, and a passing axe-core scan
- [ ] No visual spec baseline was updated without first checking its paired style-snapshot diff
- [ ] No Storybook or Chromatic dependency exists anywhere in either plateau's `package.json`
