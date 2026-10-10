# Task: Angular testing for a library repository and for an Nx workspace

Completed on branch `angular-testing-shapes`; implementation, decisions and verification are in [ANGULAR-SHAPES.md](ANGULAR-SHAPES.md).

## Why

`skills/testing/angular/solution-conformance-testing-in-angular.skill` gives an Angular project four test kinds behind the `make` contract — `unit` and `mutation` from the TypeScript parent, `components` (Angular unit-test builder, Vitest, TestBed) and `ui` (Playwright) — with a runnable example. It is delivered for one shape of repository: a single Angular CLI application.

The owner's Angular work has four situations, and an agent must be able to pick the right skill in each:

1. a simple Angular application, no packages, no Nx;
2. an application that embeds packages living in other repositories;
3. a repository holding one package, later embedded into an application that lives elsewhere;
4. an Nx workspace holding the application and its packages together.

The Angular architecture catalog (`skills/angular/architecture/`) builds on Nx. Its solutions `solution-app-testing`, `solution-ui-testing` and `solution-design-system-ui-testing` say which tests exist and where they lie in an Nx workspace; since 2026-10-09 the first two point at the testing skill for how tests are run and reported, and say that an Nx workspace needs an adapter that does not exist. This task writes it.

## The idea to build on

The contract is per repository: CI calls `make` at the repository root and gets one report. So what distinguishes the four situations is the shape of the repository, not the role of a project inside Nx:

| Situation | Shape | What differs in running the tests |
| --- | --- | --- |
| 1 and 2 | one application | nothing — a package from another repository is an npm dependency. The existing skill. |
| 3 | one library | no application of its own to open in a browser, so the `ui` kind needs a host application; the published package must hold no test file. |
| 4 | Nx workspace | the kinds run over every project at once and bring the results into one report; "only what changed" is `nx affected`. |

How a component spec or a browser spec is written is the same in all three. Only how a kind script finds and runs the tests differs.

## Decided by the owner (2026-10-09)

- **Three skills by repository shape.** The existing one stays the base and keeps the rules for writing specs and the result adapter. Two new ones refine it and replace only the kind scripts and the configuration:
  - `solution-conformance-testing-in-angular-library`
  - `solution-conformance-testing-in-angular-nx`
- **Not** one skill per Nx role (a "package" skill and an "application with Nx" skill): inside one workspace both are tested by one run and give one report.
- **Scenarios lie wherever the logic lies.** In an Nx workspace logic can be in a library and in the application itself, and both are tested: any project with framework-independent logic has its own `features/` and `test/` beside that code, by the same rule as everywhere else. The `unit` kind finds them in every project.

## Decided by default — the owner may overrule

Recommended by the previous agent; the owner has not answered.

- **One badge per kind**, with the projects broken down inside the kind's report — not a badge per library.

Open, and yours to bring to the owner with a recommendation:

- **What the mutation kind covers in a workspace**: every library with logic in a `report` run, and the affected ones in a `check` run with `DELTA_BASE`, is the obvious reading — confirm it holds with Stryker's `mutate` patterns across projects.

## What done looks like

For each of the two new skills:

1. A skill folder beside the existing one, depending on it, whose `whenToUse` lets an agent choose by one fact — what the repository holds. The three `whenToUse` texts together must leave no situation of the four without exactly one skill.
2. A runnable `example/` that passes `run-example.sh`. The owner must be able to run `make init && make test-and-report` and see the report: the same four kinds, the same landing page, the living doc with every scenario and its tags.
   - Library: a publishable Angular library with one component and some framework-independent logic; a host application the `ui` kind serves; `npm pack --dry-run` showing no spec, step or feature file in the package.
   - Nx: one application and at least two libraries, with framework-independent logic covered by scenarios in a library and in the application; a `check` run with `DELTA_BASE` in a two-commit copy showing that only the affected projects ran.
3. `# Ground truth` in the skill with the numbers you measured and a plain list of what you did not verify.
4. The kind scripts of each example compared with their originals by `check.sh` §10, as the existing Angular example is.
5. The two catalog solutions updated: their sentence "an Nx workspace needs its own adapter" becomes a link to the Nx skill. The catalog's rules are not otherwise changed.

Reuse before writing: the result adapter `assets/tools/testing/angular-results.mjs`, the Playwright config template, the component and UI spec rules, the glossary. If a refinement needs a change in the base to stay small, change the base — and re-run the base example.

## Rules of the existing skill that the new ones keep

- Angular-native specs (`*.component.spec.ts`, `*.ui.spec.ts`) lie in a `spec/` folder beside the component, as the catalog has them. Cucumber step files lie in `test/` beside `features/`, as the TypeScript parent has them.
- The `ui` kind serves the application on a free port it picks for every run (`UI_TEST_PORT`). No port number in a config.
- A screenshot baseline is reviewed and committed; a normal run never updates it (`updateSnapshots: 'none'`).
- An empty suite, a missing result or a missing browser is a red kind, not a skip.
- The TypeScript `mutation.sh` takes its delta scope from the `mutate` patterns of `stryker.conf.json`. Keep the script unchanged and express the scope in the configuration.
- `make init` installs the browser with its system libraries; it needs root or a prepared image.
- No work log in a skill, and ADRs in the format of the `adr-create` skill — both were corrected in the base skill after its first version.

## Hard parts

- **Aggregation in Nx.** Each project has its own runner output; the kind must bring them into one result, one badge and one report page without losing which project a failure belongs to. A project with no tests must not turn the kind green by being absent. Decide what an empty project means and make the red case a test of your own.
- **`nx affected` against `DELTA_BASE`.** The contract says a kind decides what `DELTA_BASE` means for it and writes that decision to its `mode`. For `components` and `ui`, which today always run in full, running only the affected projects in a `check` run is a change of behaviour — raise it with the owner before doing it.
- **Weight.** The example's own source stays small and `node_modules/` is never committed — `make init` installs it. What costs is the first run on a clean checkout: the download and the Nx start-up. Measure a cold `make init && make test-and-report` and put the time into the skill; if it passes ten minutes, say so and propose what to cut.
- **The library's host application.** The catalog already has such hosts — `apps/component-preview` for the monolith, `projects/demo` for the design system. Follow one of them rather than inventing a third.
- **Nx caching.** A cached target replays an old result. A test kind must run its tests; find how the kind keeps Nx from answering from cache, and prove it by changing a spec between two runs.

## Not in this task

- The rules for what a component test or an end-to-end test asserts: the catalog's.
- `skills/devops/`: no workflow, no change filter. Note what a workflow would need — the Nx cache, the browser image — in `skills/devops/agent/CONTEXT.md` under its known gaps.
- The `.devcontainer` image of the examples: the container this work runs in has no Docker, so it cannot be built here. Say so in the ground truth.

## The environment

Node 24, Chromium through Playwright, no Docker. The base example's last measured run: 25/25 scenarios, domain coverage 98.1%, mutation 90.2%, components 4/4, UI 4/4.
