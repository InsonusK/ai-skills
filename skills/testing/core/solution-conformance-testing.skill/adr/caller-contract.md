---
name: caller-contract
description: What a caller of a project's tests — a developer, an agent, a CI workflow — may rely on, so it never learns which test kinds exist or how they run
problem: CI workflows named the test targets, set coverage and delta switches, passed `tmp/` between jobs and knew three badge file names — every new test kind or tool change meant changing every workflow; what is the smallest contract a caller needs?
decision: Independent `make test-kind-{kind}` targets discovered through `make test-kinds`, then `make test-report`; the caller passes facts about the run (`TEST_RUN_PURPOSE`, `DELTA_BASE`) and chooses the work and report directories; the report is `index.html`, `reports/{name}/`, `badges/{name}.json`; README badges are checked against the declared ones, not written by CI.
tags:
  - solution/conformance-testing
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The contract was four fixed targets — `unit-test`, `mutation-test`, `test-report`, `test-and-report` — with `WITH_CODE_COVERAGE`, `ONLY_DELTA`, `DELTA_BASE`, results in `tmp/`, and the report in `public/`. The CI workflows built on it ran `unit-test` and `mutation-test` as named jobs, decided themselves that a pull request skips coverage and mutation, passed `tmp/result` and `tmp/report/{tests,coverage,mutation}` between jobs, and put three badge file names into the README. A caller therefore knew the kinds, their switches, their intermediate files and their metrics; adding a kind or changing a tool reached every workflow, and a project already using `public/` for its own site collided with the report. What is the smallest contract a caller needs?

# Selected variant
[[#Discovered kinds, facts about the run, caller-chosen directories]]

# Searched variants

## Discovered kinds, facts about the run, caller-chosen directories

**Selected.**

### Description
- **Targets.** `make test-kinds` lists the kinds and the badges each declares; `make test-kind-{kind}` runs one, independently of the others; `make test-report` builds the report; `make test-readme-check` checks the README badges; `make test-and-report` is the local all-in-one.
- **Input.** `TEST_RUN_PURPOSE` = `pr-check` (decides whether a pull request may merge; must be fast) or `report` (builds the full reports and badges), and `DELTA_BASE`. They are facts; each kind decides what they mean for it, logs that decision, and records it in the report — or skips itself with a reason.
- **Directories.** The caller sets `TEST_WORK_DIR` and `TEST_REPORT_DIR`; a kind writes only to `{work}/kinds/{kind}/`.
- **Output.** `index.html`, `reports/{name}/`, `badges/{name}.json` (a badge and its report share a name), `run.json`.
- **README.** Whoever adds a kind adds its badge; `test-readme-check` fails naming a missing or stale badge, comparing against the declared badges.
- The caller-facing half is one file, `tools/testing/testing.mk`, identical in every stack.

### Benefits
- A new kind is a declaration and a recipe in the project; no caller changes, and CI still gets one job and one status per kind, in parallel.
- What a pull request skips is decided next to the tool that knows the cost, and is visible in the log and the report.
- The report can be written into a subfolder of a site the project already publishes.

### Costs
- A dynamic job matrix in CI instead of named jobs.
- A kind's self-skip must be trusted; the marker and `run.json` make it visible, not impossible.
- README badges still need a manual line per badge.

## One target for everything

### Description
A single `make test-and-report` that runs all kinds and builds the report; the caller runs nothing else.

### Benefits
- The smallest possible contract.

### Costs
- One CI job and one status for all kinds, run in sequence; a slow kind delays the fast one's feedback.

## Keep the fixed targets and switches

### Description
The status quo: four named targets and tool switches set by the caller.

### Benefits
- Nothing to migrate; workflows are easy to read.

### Costs
- The problem as stated: every caller knows every kind.

## CI writes the README badges

### Description
The workflow rewrites the README's badge block from the badges the run produced.

### Benefits
- No manual badge line.

### Costs
- The workflow needs write access to a protected branch and changes the README outside review, for an event — a kind added or removed — that is rare.
