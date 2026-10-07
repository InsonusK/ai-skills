---
description: The stack-independent half of the testing contract — caller variables, the kind list, the report and README checks
element_kind: file
change_kind: create
tags:
  - solution/conformance-testing
  - element/tools-testing-testing-mk
---

# Goals
- Give every project the same caller-facing targets and variables, so a caller never learns which test kinds exist or how they run.

# Core Principles
- The project's `Makefile` declares its kinds and their badges, includes this file, and defines only `test-kind-{kind}` and `test-report-build`; everything a caller touches is defined here.
- `TEST_RUN_PURPOSE` and `DELTA_BASE` state facts about the run. This file passes them on; each kind decides what they mean for it and says so through `test-kind-mode` or `test-kind-skip`.
- A kind writes only below `$(TEST_KIND_DIR)` = `$(TEST_WORK_DIR)/kinds/{kind}`, so kinds run in parallel and a CI job hands its one directory to the report job.

# Implementation changes
`tools/testing/testing.mk`:
Copy verbatim to `tools/testing/testing.mk`: [`assets/tools/testing/testing.mk`](../../../assets/tools/testing/testing.mk)

# Rule changes

## MUST
- Copy this file verbatim into every project; never edit it there.
  - Risk: a project-local change makes the caller-facing contract differ between projects.
  - Fix: put anything project- or stack-specific in the project's `Makefile`.
- Set `TEST_KINDS` and every `TEST_BADGES_{kind}` before the `include` line.
  - Risk: the kind list and the badge declaration are computed when this file is read; a later assignment is ignored.
  - Fix: declare first, include second, define the `test-kind-{kind}` targets third.

# Check list
- [ ] `tools/testing/testing.mk` is a byte-for-byte copy of the asset.
- [ ] `make test-kinds` prints one line per kind with its badges and runs nothing.
