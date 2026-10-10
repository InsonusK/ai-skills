---
name: devops-ci-orchestration
description: What a CI workflow may contain and what it must call — testing, test reports, and the project's version only through the project's make targets, runnable locally; change detection, tags, and release text copied ready-made from the skill that ships them
whenToUse: when you write, change, or review a CI workflow or a composite action of a project, or decide whether a CI step belongs in the workflow file or behind a `make` target
updated: 20261010
tags:
  - stack
  - concern/ci
  - github-actions
adr:
  - adr/make-for-what-changes.md
---

# Goal
- Every workflow step that runs tests, builds the test report, or reads or checks the version is one call of a `make` target from [[#Call only the caller contract|The caller contract]].
- Every part decided once per stack — the change-detection action, the workflow file — is a file copied from the skill that ships it.
- No workflow or action names a test tool, a coverage or mutation switch, or the file the version is recorded in.

# Core Principle
- **Local first** - A step that changes with the project fails on a developer's machine before it fails on a runner, because the workflow runs the same `make` target the developer runs.
- **Copied, not written** - A step settled once per stack is delivered as a ready file, so every project has the same one. Decision recorded in [[./adr/make-for-what-changes.md|make-for-what-changes]].
- A workflow states facts about the run; the project's scripts decide what they mean.

# Rule

## MUST

### Call only the caller contract
Reach testing and the version only through these targets and variables.

| Target | Does | Owner |
| --- | --- | --- |
| `make init` | prepares a clean checkout once the toolchain is installed: dependencies, tools, browsers | the project's `Makefile` |
| `make version` | prints the version, `MAJOR.MINOR.PATCH` | [[skills/devops/core/devops-project-version.skill/devops-project-version.skill.md\|devops-project-version]] |
| `make version-check` | fails unless the version is greater than the one at `DELTA_BASE` | the same |
| `make test-kinds`, `make test-kind-{kind}`, `make test-report`, `make test-readme-check` | list the test kinds, run one, build the report, check the README badges | [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md\|solution-conformance-testing]] |

Variables: `DELTA_BASE`, `TEST_RUN_PURPOSE`, `TEST_WORK_DIR`, `TEST_REPORT_DIR`.
- Violation: `run: go test ./...`, `run: cat VERSION`, or a new `make coverage` target called from a workflow.
- Risk: the workflow breaks when the project changes a tool or a file, and the step cannot be tried before a push.
- Fix: call the target; a need the contract does not cover is raised with the user, not added to one workflow.

### Never name a test tool or a version source
Keep every test tool, test kind, coverage or mutation switch, and version file name out of workflows and actions.
- Violation: a job named `mutation`, `--coverage` in a step, `hashFiles('package.json')` used to read the version.
- Risk: adding a test kind or moving the version becomes a change to every workflow of every project.
- Fix: discover the kinds with `make test-kinds`; read the version with `make version`.

### State facts, never switches
Pass a workflow's knowledge of the run as `TEST_RUN_PURPOSE` (`check` or `report`) and `DELTA_BASE` (a ref), and nothing else.
- Violation: `COVERAGE=1`, `SKIP_MUTATION=true`, or a script reading `GITHUB_REF`.
- Risk: a switch duplicates a decision the project's scripts own, and a script that reads a platform variable no longer runs locally.
- Fix: set the two variables in the workflow; let each script decide what they mean for it.

### Copy what is decided once
Take the change-detection action and each workflow file from the skill that ships it, and change only what that skill lists as a placeholder.
- Violation: path patterns or a tag string typed into a workflow from memory.
- Risk: each project gets a slightly different copy, and a fix in the skill reaches none of them.
- Fix: copy the file; propose a needed deviation to the user and fold it back into the skill's file.

### Toolchain in the workflow, the rest in make init
Install the stack's toolchain with the workflow's setup action, then run `make init`.
- Violation: `npm ci`, `go install …`, or `playwright install` as workflow steps.
- Risk: the list of what a checkout needs lives in the workflow, and a developer's machine is prepared differently.
- Fix: keep one setup step for the toolchain and its cache; move every other preparation into the `init` target.

### Start test services from the dev container's compose file
Start a database or another service the tests need from `.devcontainer/docker-compose.yml`, before the test step.
```yaml
      - run: docker compose --file .devcontainer/docker-compose.yml up --detach --wait {test-services}
```
`{test-services}` — the compose service names the tests need, never the dev container itself.
- Violation: a `services:` block in the workflow describing the same database a second time.
- Risk: CI and the dev container run different versions or settings of the service, and a test passes in one only.
- Fix: one compose file; the services publish their ports, and the job's `env` gives the tests the same variables the dev container sets, with `localhost` as the host.

# Check list
- [ ] Every test, report, and version step is a `make` target of [[#Call only the caller contract|The caller contract]].
- [ ] No workflow or action names a test tool, a test kind, a coverage or mutation switch, or a version file.
- [ ] The workflow sets only `TEST_RUN_PURPOSE`, `DELTA_BASE`, and the two test directories; no script reads a `GITHUB_*` variable.
- [ ] The change-detection action and the workflow files are copies of their skills' files, differing only in listed placeholders.
- [ ] Preparation beyond the toolchain is in `make init`.
- [ ] Services for tests are started from `.devcontainer/docker-compose.yml`; the workflow has no `services:` block.
