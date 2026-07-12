---
name: accessibility-testing-approach
description: How component-level accessibility regressions are caught automatically, rather than relying solely on manual review
problem: Neither behavioral component tests (Testing Library) nor visual screenshot tests assert WCAG-level accessibility rules (color contrast ratios, missing labels/roles, invalid ARIA usage); without an automated check, accessibility regressions only surface if a reviewer happens to notice them
decision: @axe-core/playwright, run against each plateau's demo/preview pages, as an automatic per-component/per-state check
---

# Problem

Testing Library-based component tests (see [[skills/angular/architecture/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create]]) do encourage accessible queries (`getByRole`, `getByLabelText`), which catches the *absence* of an accessible name/role a test author chose to query for — but that is incidental, not exhaustive: it only catches what the test happens to assert, not the rules a component actually needs to satisfy (contrast ratio, redundant/conflicting ARIA attributes, focus order, landmark structure). A dedicated automated accessibility check, run against the same rendered output already used for visual regression testing, closes this gap without requiring a human to manually run a screen reader or contrast checker on every change.

# Selected variant

**Selected variant:** [[#@axe-core/playwright against demo/preview pages]]

Every component/state covered by a visual regression spec (see [[skills/angular/architecture/solutions/solution-ui-testing.skill/adr/visual-regression-approach]]) also gets an `@axe-core/playwright` scan against the same demo/preview page, asserting zero violations. This reuses the same Playwright + demo/preview-page infrastructure the visual regression decision already established — no separate harness.

# Searched variants

## @axe-core/playwright against demo/preview pages

### Description

`@axe-core/playwright` injects the axe-core accessibility engine into the same Playwright page already loaded for a component's screenshot test, runs a scan, and asserts the returned violations array is empty (or contains only explicitly, individually acknowledged exceptions).

### Benefits

- Reuses the exact same Playwright page/navigation already set up for visual regression testing — no second browser-automation harness to maintain
- axe-core is the same accessibility-rule engine used by most major automated a11y tools (including browser DevTools' own Lighthouse/axe integration), so violations found here match what a manual audit with common tooling would also find
- Runs automatically on every PR touching a component, rather than depending on a reviewer remembering to check accessibility by hand
- Catches an entire class of regression neither the behavioral component test nor the visual screenshot test can: axe-core specifically evaluates contrast ratios, ARIA validity, and landmark/heading structure against WCAG rules — a screenshot can look "visually fine" to a sighted reviewer while still failing several of these

### Costs

- axe-core (like all automated accessibility tools) cannot catch everything — it typically catches roughly a third to half of real WCAG issues; genuine issues requiring judgment (is this alt text actually meaningful, does this focus order make sense) still need occasional manual review
- Adds one more assertion per component/state to maintain, and an explicit-exception mechanism is needed for any rule a specific component deliberately violates (rare, but must not be silently ignored workspace-wide)

## Manual accessibility review only

### Description

Rely on code review and periodic manual audits (screen reader testing, manual contrast checks) with no automated per-PR check.

### Benefits

- No new dependency or CI step to maintain
- Manual review can catch nuanced issues (meaningful alt text, sensible focus order) automated tools cannot

### Costs

- Entirely dependent on a reviewer remembering to check, and having the expertise to check correctly, on every single PR — the exact gap this decision exists to close
- No safety net at all for the mechanically-checkable subset of WCAG rules (contrast, ARIA validity) that a tool can catch reliably and cheaply

## A dedicated accessibility-testing SaaS/platform (e.g. axe DevTools cloud, Deque)

### Description

Adopt a paid, cloud-hosted accessibility-scanning service with its own dashboard, historical trend tracking, and CI integration.

### Benefits

- Hosted dashboards, trend tracking over time, and typically a broader rule set / additional manual-audit services bundled in

### Costs

- New vendor relationship and billing, for a capability the free, self-hosted `@axe-core/playwright` package already provides for the specific need here (per-component/per-PR regression catching)
- Same category of objection already raised against Chromatic in [[skills/angular/architecture/solutions/solution-ui-testing.skill/adr/visual-regression-approach]] — introduces an external dependency for a check that can be run entirely in-house with tooling already adopted
