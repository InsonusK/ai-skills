---
name: visual-regression-approach
description: How visual (screenshot) regressions in UI components are caught, given behavioral component tests (Testing Library) already exist but cannot detect layout/rendering regressions
problem: jsdom/happy-dom (used by behavioral component tests) has no layout engine, so a component test can confirm the right text/roles are present but cannot detect a broken layout, a broken dark-mode branch, lost contrast, or a CSS-specificity conflict
decision: Playwright screenshot testing (toHaveScreenshot()) directly against each plateau's own demo/preview pages, without Storybook or Chromatic
tags:
  - solution/ui-testing
  - concern/documentation
  - concern/documentation/adr
---

# Problem

[Behavioral component tests](skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/glossary/behavioral-component-testing.md) (Testing Library, running against jsdom/happy-dom — see [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create]]) verify that a component renders the right accessible content and reacts correctly to input/output changes. They cannot verify what the component actually *looks like*: jsdom/happy-dom has no layout engine, so a test can assert `getByRole('button')` exists but has no way to answer "is this button visually broken" — a regression in CSS Grid/Flexbox layout, a `light-dark()` branch that silently stopped applying, lost color contrast, or a specificity conflict from a newly added global style. These are exactly the [visual regressions](skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/glossary/visual-regression-testing.md) most likely to slip through unnoticed, because they're visible to a human glancing at the page but invisible to a DOM-structure assertion. We need a way to catch them automatically, in CI, without a human reviewing every PR's rendered output by eye.

# Selected variant

**Selected variant:** [[#Playwright screenshot testing against demo/preview pages]]

Every component that ships a demo/preview page (design system: `projects/demo`; platform: `apps/component-preview`, see [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/PlatformComponents/component-preview.project.create]]) gets a Playwright `spec/{component-name}.visual.spec.ts` asserting `expect(page).toHaveScreenshot()` against that page, in both light and dark color schemes. Baseline PNGs are committed under `spec/snapshot/`. No Storybook, no Chromatic — screenshots are taken directly against the same demo/preview app already used for manual visual review, using Playwright, which this architecture has already adopted for e2e testing (see [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/adr/e2e-framework-choice.md|e2e-framework-choice ADR]]).

# Searched variants

## Playwright screenshot testing against demo/preview pages

### Description

Use Playwright's own built-in screenshot assertion (`expect(page).toHaveScreenshot()`) directly against the plateau's existing demo/preview pages. A baseline screenshot is committed per component/state/color-scheme; CI fails on pixel diff beyond a configured threshold; an engineer explicitly re-runs with `--update-snapshots` to accept an intentional visual change, which then goes through normal code review as a diff to the committed baseline image.

### Benefits

- No new tool or vendor: Playwright is already the adopted e2e framework (see [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/adr/e2e-framework-choice.md|e2e-framework-choice]]) — this is the same tool, a different assertion
- No dependency on Storybook, which was already evaluated and rejected for this exact team on this exact kind of work (see [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/adr/component-preview-tooling.md|component-preview-tooling ADR]]) — adopting Chromatic would require reversing that decision, since Chromatic's snapshot pipeline is built on Storybook stories as its input, not arbitrary pages
- Baselines are plain image files committed to the repository — no external SaaS, no design-system screenshots leaving the org's own infrastructure
- Runs entirely in CI the team already operates; no new billing relationship, no new vendor risk
- A visual diff is reviewed the same way any other diff is reviewed — as part of the PR, not in a separate external UI

### Costs

- No hosted diff-review UI — reviewing a screenshot diff means looking at the two committed images (or a generated diff image) in the PR, rather than a purpose-built comparison tool
- Baseline images add binary file weight to the repository over time; needs a sane retention/cleanup convention as components are removed or restructured
- The team owns the harness (baseline update workflow, threshold tuning, flakiness from font-rendering/anti-aliasing differences across CI runners) instead of a vendor owning it — mitigated by pinning the CI runner's OS/browser build and Playwright's own configurable diff threshold

## Storybook + Chromatic

### Description

Adopt Storybook as the component-authoring/preview format (stories), then use Chromatic (a paid, cloud-hosted service built specifically on top of Storybook) to capture and diff screenshots per story on every PR, with a hosted UI for reviewing and accepting visual changes.

### Benefits

- Purpose-built hosted diff-review UI, with per-change accept/reject workflow, historical trends, and cross-browser cloud rendering
- Wide ecosystem adoption; well-documented for teams already using Storybook

### Costs

- **Requires Storybook as a mandatory foundation, not an optional add-on** — Chromatic's entire snapshot pipeline is driven by Storybook stories; adopting it means adopting Storybook first, which this architecture has already evaluated and explicitly rejected for this exact use case (see [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/adr/component-preview-tooling.md|component-preview-tooling ADR]]) — this would be reversing a real, first-hand decision, not a hypothetical trade-off
- Direct, first-hand prior experience with Storybook + Angular on this team specifically produced significant friction and configuration overhead that was never fully resolved (auto-test setup was never gotten working) — the exact failure mode this decision is trying to avoid repeating
- Paid, cloud-hosted SaaS: a new vendor relationship, new billing, and design-system screenshots (potentially including pre-release UI) leaving the org's own infrastructure to a third-party's servers
- A practitioner with several years of hands-on Chromatic experience, evaluating it independently, concluded there was nothing it provided that couldn't be replicated directly with Playwright, and migrated off it to a self-built Playwright-based solution — external validation that this capability doesn't require the vendor lock-in
- Adds a second, largely redundant test-runner surface (Chromatic's own CI integration) alongside the Playwright e2e suite this architecture already runs

## No visual regression testing

### Description

Rely solely on behavioral component tests (Testing Library) and manual visual review during code review; do not add any automated screenshot-based check.

### Benefits

- Zero additional tooling, zero baseline-image maintenance burden
- No risk of screenshot flakiness (font rendering, anti-aliasing) generating false-positive CI failures

### Costs

- The exact class of regression this ADR exists to catch — layout breakage, broken dark-mode, lost contrast, CSS-specificity conflicts — has no automated safety net at all, and depends entirely on a human noticing it during manual review, which is exactly the gap that motivated this decision
- As the component count grows, manual visual review of every change becomes proportionally less reliable and more time-consuming
