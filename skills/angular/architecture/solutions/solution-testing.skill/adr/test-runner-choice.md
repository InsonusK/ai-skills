---
name: test-runner-choice
description: Choice of test runner for unit and component tests
problem: Which test runner to standardize on, given the workspace is upgrading to Angular 22 (for the Signal Forms solution) and the previous team choice (Jest) predates that decision
decision: Use Vitest, Angular's own default test runner as of Angular 21+
---

# Problem

The team had previously moved from Karma/Jasmine to Jest, but Angular's own tooling direction has since moved further: Vitest became the framework's default test runner starting with Angular 21, with Karma's built-in support removed. Given the "Формы" solution already commits this workspace to Angular 22, the test runner choice should be reconsidered against Angular's own current default rather than carried over from a decision made before that direction was set.

# Selected variant

**Selected variant:** [[#Vitest]]

Vitest is adopted as the test runner for all unit and component tests, matching Angular's own current default and its native esbuild-based build pipeline (already in use per the "Структура репозитория" solution's Nx/Angular CLI foundation).

# Searched variants

## Vitest

### Description

Angular CLI's own default test runner from Angular 21 onward, built on Vite/esbuild, with native ESM support and no separate transform step for TypeScript.

### Benefits

- Matches Angular's own current default and tooling direction — no fighting the framework's own trajectory
- Substantially faster test execution than Karma (commonly cited at several times faster), and generally faster startup/watch-mode than Jest's CommonJS-oriented transform pipeline, since it reuses the same esbuild-based pipeline the application itself already builds with
- Native ESM support avoids the CommonJS/ESM interop friction Jest has historically needed extra configuration to work around in Angular projects
- Actively maintained as Angular's forward-looking default, meaning future Angular tooling improvements will assume it rather than treat it as a third-party add-on

### Costs

- Newer than Jest in the Angular ecosystem — less accumulated community troubleshooting content for edge cases specific to this pairing
- Requires migrating any test tooling/config previously written for Jest (custom matchers, setup files) to Vitest's equivalents

## Jest

### Description

Continue with the team's previous choice; Angular has experimental Jest support but it is not the framework's own default going forward.

### Benefits

- No migration needed from what the team already had configured
- Very large ecosystem and community familiarity

### Costs

- Requires ongoing manual configuration to work well with Angular's esbuild-based `ApplicationBuilder`, rather than being supported as a first-class default
- Diverges from Angular's own stated tooling direction, meaning future framework-level testing improvements are less likely to be designed with Jest in mind
- Generally slower than Vitest for the same test suite, given Jest's CommonJS-oriented transform pipeline

## Karma/Jasmine

### Description

The long-standing original Angular default, since replaced by Vitest as of Angular 21.

### Benefits

- Longest history and the most legacy documentation/tutorials

### Costs

- No longer Angular's default or actively promoted path — the framework has moved away from it
- Meaningfully slower than both Vitest and Jest for typical suites
