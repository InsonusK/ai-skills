---
name: devops-ci-toolchain
description: How a CI job gets the project's toolchain — one setup-toolchain composite action, shipped ready-made per stack, that installs the version the project itself declares, so that workflow files stay identical across stacks
whenToUse: when a CI job must install a language toolchain before `make init`, or when you add or review `.github/actions/setup-toolchain/action.yml`
updated: 20261010
tags:
  - stack
  - concern/ci
  - github-actions
---

# Goal
- `.github/actions/setup-toolchain/action.yml` in the project, an unchanged copy of the stack extension's asset.
- Every job that builds or tests calling `./.github/actions/setup-toolchain`, then `make init`.
- No toolchain version written in a workflow or an action.

# Core Principle
- **One stack-specific step** - The action is the one stack-specific step of a workflow; with it behind a fixed name, the workflow templates need no change per stack.
- **Stack extensions** - The ready action for a stack is in `devops-ci-toolchain-in-go`, `devops-ci-toolchain-in-python`, `devops-ci-toolchain-in-typescript` (also for Angular), `devops-ci-toolchain-in-dotnet`; ask the user which one to load.

# Rule

## MUST

### Copy the stack's action verbatim
Copy `action.yml` from the extension for the project's stack to `.github/actions/setup-toolchain/action.yml`, and do not modify it.
- Violation: `actions/setup-go` called directly in a workflow.
- Risk: the workflow becomes stack-specific and is no longer the file every project copies.
- Fix: call `./.github/actions/setup-toolchain` after the checkout.

### Node for the test report in every stack
The action of a stack other than Node's also installs Node, in the version `tools/livingdoc/package.json` declares in `engines`.
- Violation: a Go project whose runner renders the living doc with whatever Node the image carries; `actions/setup-node` added to a workflow.
- Risk: the renderer of [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] needs Node 22 or newer; on an older one the tests are green and the `tests` link of the published report leads nowhere.
- Fix: keep the `actions/setup-node` step of the stack's asset; it reads the version from the renderer's own manifest.

### Only the toolchain and its cache
Keep everything except the toolchain install and its dependency cache out of the action.
- Violation: `npm ci` or a browser install added to the action.
- Risk: a developer's `make init` and the runner prepare a checkout differently.
- Fix: put the preparation into the `init` target of the `Makefile`.

# Check list
- [ ] `.github/actions/setup-toolchain/action.yml` is byte-identical to the stack extension's asset.
- [ ] Every building or testing job calls the action and then `make init`.
- [ ] No workflow or action holds a toolchain version.
- [ ] The action installs Node from `tools/livingdoc/package.json`, or the stack's own toolchain is Node.
