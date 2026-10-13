---
name: one-release-workflow
description: How many workflows start on a push to develop or master, and what creates the GitHub Release
problem: A push to master started four workflows — Docker publish, package publish, release record, test report — each detecting changes and reading the version, three of them running the tests. Should a push start one workflow or one per delivery?
decision: One `release.yml`, the same file in every repository; what a project delivers is one composite action at a fixed path, `.github/actions/release`, taken from the skill of that delivery; the tag `v{version}` and the Release are created by the workflow's last job for every project type.
tags:
  - stack
  - concern/ci
  - concern/documentation
  - concern/documentation/adr
---

# Problem
With one workflow per delivery, the shared jobs were kept identical by copying, the tests ran in each workflow, and the release record computed links to artifacts of workflows it could not wait for. Should a push start one workflow or several?

# Selected variant
[[#One workflow, delivery behind one action]]
- Decided with the owner on 2026-10-10: one file, test kinds as parallel jobs, a Release for every project type, coverage and the report only on `master`.

# Searched variants

## One workflow, delivery behind one action

**Selected.**

### Description
`release.yml` holds `changes`, `version`, the test jobs, the report, a `deliver` job, and a final `release` job. `deliver` calls `./.github/actions/release` with the channel, the version, a timestamp, and two registry tokens; the action returns lines for the Release text and leaves files to attach in `dist/release`. One skill per kind of delivery ships that action. The pull-request workflow calls the same action with `channel: check`.

### Benefits
- Both workflow files are copied verbatim and are the same in every project.
- Changes, the version, and the tests are computed once per push.
- A pull request builds what a release will publish — image, package, or binaries.
- A new kind of delivery is a new skill with one file; no workflow changes.

### Costs
- A composite action cannot declare permissions or read secrets: the `deliver` job holds the union of permissions, and the registry tokens have two generic secret names.
- A project has one release action; delivering two things needs an action written for it.

## One workflow assembled from marked blocks

### Description
`release.yml` as a template with `docker`, `package`, and `app` blocks that a script keeps or removes, a stack's job passed to the script as a file.

### Benefits
- Each delivery is its own job with its own permissions and secrets.
- A project can keep several deliveries.

### Costs
- The workflow differs between projects and is produced by a script, not copied.
- Structural markers in a template, and a script to maintain and explain.
- The pull request checks only the image build.

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
