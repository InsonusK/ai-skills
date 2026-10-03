---
name: solution-taskbox
description: Pointer to the TaskBox (VP-C003) specification repository — its contract, conformance feature, and the agent skills for using TaskBox in a service or building a TaskBox library
whenToUse: when a backend web-service must run work later, retry it until it succeeds, or keep it in order per key — in particular work enqueued atomically with a data change — or when building or reviewing a TaskBox library for a stack
domain: skill
type: architecture
version: 20260929000000
updated: 20260929
tags:
  - skill/architecture/solution
  - solution/taskbox
  - stack
  - concern/architecture
  - taskbox
creates:
extends:
depends_on:
built_on_plateau:
adr:
---

# Goal
- The TaskBox specification repository [InsonusK/taskbox-spec](https://github.com/InsonusK/taskbox-spec) loaded as the source for the task at hand — its contract, conformance feature, and `doc/skills/`.

# Core Principle
- **TaskBox lives in its own repositories** - VP-C003 is a contract VP, delivered as a spec repository plus one library repository per stack ([[skills/common-workflow/architecture/design/solution-create.skill/adr/contract-vp-realized-by-libraries|solution-create ADR]]); this skill holds no rule of its own, so it cannot drift from them.
- The VP concept — when TaskBox is needed, criticality and stores — stays in [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/vp/vp-c003-taskbox/vp-c003-taskbox|VP-C003 TaskBox]].

# Rule

## MUST

### Load the repository's skill for the task
Open `taskbox-spec` and follow the skill in its `doc/skills/` that matches the task: `taskbox-usage` when a service uses TaskBox, `taskbox-library` when building or releasing a library.
- Risk: working from memory or from an older copy of the rules breaks guarantees the conformance feature proves (group order, lease fencing, retention).
- Fix: read the skill at the spec release the service's library pins.

### Use the stack's library
Use the TaskBox library of the service's stack at a conforming release; never implement the mechanism in the service.
- Risk: a hand-written mechanism is unproven and drifts from the contract.
- Fix: the libraries are listed in `taskbox-spec`'s README; ask the user which stack's solution to load (`solution-taskbox-in-go`, …) for the service.

# Check list
- [ ] The matching `doc/skills/` skill of `taskbox-spec` was applied.
- [ ] The service depends on its stack's TaskBox library at a release naming its conformed spec version.
