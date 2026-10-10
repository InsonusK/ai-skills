---
name: one-release-workflow
description: How many workflows start on a push to develop or master, and what creates the GitHub Release
problem: A push to master started four workflows — Docker publish, package publish, release record, test report — each detecting changes and reading the version, three of them running the tests. Should a push start one workflow or one per delivery?
decision: One `release.yml` per repository; the project's deliveries are jobs of it, chosen when the file is assembled; the tag `v{version}` and the Release are created by its last job for every project type.
tags:
  - stack
  - concern/ci
  - concern/documentation
  - concern/documentation/adr
---

# Problem
With one workflow per delivery, the shared jobs were kept identical by copying, the tests ran in each workflow, and the release record computed links to artifacts of workflows it could not wait for. Should a push start one workflow or several?

# Selected variant
[[#One workflow, delivery jobs by project type]]
- Decided with the owner on 2026-10-10: one file, test kinds as parallel jobs, a Release for every project type, coverage and the report only on `master`.

# Searched variants

## One workflow, delivery jobs by project type

**Selected.**

### Description
`release.yml` holds `changes`, `version`, the test jobs, the report, and a final `release` job. `image`, `package`, and `app` are marked blocks the assembling script keeps or removes; a stack ships its `package` or `app` job as a file.

### Benefits
- Changes, the version, and the tests are computed once per push.
- The `release` job waits for the deliveries and attaches what they produced.
- One place to read what a push does.

### Costs
- The file is assembled, not copied; adding a delivery means assembling it again.
- A failed job is re-run inside one larger workflow.

## One workflow per delivery

### Description
`docker-release-publish.yml`, `stack-lib-release-publish.yml`, `release-info-publish.yml`, `release-test-report.yml`, each with its own copy of the shared jobs.

### Benefits
- A project adds a delivery by adding a file.
- Each workflow is short.

### Costs
- The tests run in three workflows on one push.
- The shared jobs are kept identical by hand.
- The Release is created without knowing whether the image was published.

## A reusable workflow called by thin callers

### Description
The shared jobs live in a `workflow_call` workflow; each delivery is a caller.

### Benefits
- No copied jobs.

### Costs
- Each caller still runs the shared jobs once, so the tests still run per delivery.
- Outputs cross workflow boundaries only through artifacts or the API.
