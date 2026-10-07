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
```makefile
# tools/testing/testing.mk - the stack-independent half of the testing contract
# (solution-conformance-testing). Copied verbatim into every project; never edited there.
#
# The project's Makefile, before including this file, sets
#   TEST_KINDS          := unit mutation          every test kind
#   TEST_BADGES_<kind>  := tests coverage         the badges that kind produces in a full run
# and, after including it, defines one target per kind and the report builder:
#   test-kind-<kind>:    runs the kind, writing only below $(TEST_KIND_DIR)
#   test-report-build:   turns $(TEST_WORK_DIR)/kinds/* into $(TEST_REPORT_DIR)
#
# Callers (a developer, CI) use only:
#   make test-kinds | test-kind-<kind> | test-report | test-readme-check | test-and-report
#   TEST_RUN_PURPOSE=pr-check|report   DELTA_BASE=<ref>   TEST_WORK_DIR=<dir>   TEST_REPORT_DIR=<dir>

# What the run is for. pr-check: decides whether a pull request may merge - must be fast.
# report: builds the full reports and badges for publishing.
TEST_RUN_PURPOSE ?= report
# The ref to compare against when a kind can limit itself to changed code. Empty: none.
DELTA_BASE ?=
# Where kinds write their results: $(TEST_WORK_DIR)/kinds/<kind>/.
TEST_WORK_DIR ?= tmp/testing
# Where test-report writes the publishable report.
TEST_REPORT_DIR ?= $(TEST_WORK_DIR)/report
# The file test-readme-check reads.
TEST_README ?= README.md

ifeq ($(filter $(TEST_RUN_PURPOSE),pr-check report),)
$(error TEST_RUN_PURPOSE must be pr-check or report, got '$(TEST_RUN_PURPOSE)')
endif

# "<kind>:<badge>,<badge> ..." - the declaration tools/testing/testing.sh checks against.
comma := ,
empty :=
space := $(empty) $(empty)
TEST_DECLARED := $(foreach k,$(TEST_KINDS),$(k):$(subst $(space),$(comma),$(strip $(TEST_BADGES_$(k)))))

export TEST_RUN_PURPOSE DELTA_BASE TEST_WORK_DIR TEST_REPORT_DIR TEST_README TEST_DECLARED

# Inside a test-kind-<kind> recipe: the only directory the kind may write to.
test-kind-%: export TEST_KIND_DIR = $(TEST_WORK_DIR)/kinds/$(patsubst test-kind-%,%,$@)

# First line of every test-kind-<kind> recipe: start from an empty kind directory.
define test-kind-begin
rm -rf "$(TEST_KIND_DIR)" && mkdir -p "$(TEST_KIND_DIR)/result" "$(TEST_KIND_DIR)/report"
endef

# $(call test-kind-mode,<what the kind does because of the run's purpose>) - to the log and the report.
define test-kind-mode
echo "$@ [purpose $(TEST_RUN_PURPOSE)]: $(1)" && echo "$(1)" > "$(TEST_KIND_DIR)/mode"
endef

# $(call test-kind-skip,<reason>) - the kind does not apply to this run: no result, no badge, exit 0.
define test-kind-skip
echo "$@ [purpose $(TEST_RUN_PURPOSE)]: skipped - $(1)" && echo "$(1)" > "$(TEST_KIND_DIR)/skipped" && exit 0
endef

.PHONY: test-kinds test-report test-report-build test-readme-check test-and-report $(addprefix test-kind-,$(TEST_KINDS))

# One line per kind: "<kind> <badge> <badge> ...". Runs nothing.
test-kinds:
	@$(foreach k,$(TEST_KINDS),echo "$(k) $(strip $(TEST_BADGES_$(k)))";)

# Fails when the README lacks a declared badge or shows an undeclared one. Runs no tests.
test-readme-check:
	@bash tools/testing/testing.sh readme-check

# Builds $(TEST_REPORT_DIR) from whatever the kinds left in $(TEST_WORK_DIR)/kinds.
test-report:
	@rm -rf "$(TEST_REPORT_DIR)" && mkdir -p "$(TEST_REPORT_DIR)/reports" "$(TEST_REPORT_DIR)/badges"
	@$(MAKE) --no-print-directory test-report-build
	@bash tools/testing/testing.sh report-finish

# Local convenience: every kind, then the report. Exits non-zero when any step failed.
test-and-report:
	@status=0; \
	for k in $(TEST_KINDS); do $(MAKE) --no-print-directory test-kind-$$k || status=$$?; done; \
	$(MAKE) --no-print-directory test-report || status=$$?; \
	exit $$status
```

# Rule changes

## MUST
- Copy this file verbatim into every project; never edit it there.
  - Risk: a project-local change makes the caller-facing contract differ between projects.
  - Fix: put anything project- or stack-specific in the project's `Makefile`.
- Set `TEST_KINDS` and every `TEST_BADGES_{kind}` before the `include` line.
  - Risk: the kind list and the badge declaration are computed when this file is read; a later assignment is ignored.
  - Fix: declare first, include second, define the `test-kind-{kind}` targets third.

# Check list
- [ ] `tools/testing/testing.mk` matches this file.
- [ ] `make test-kinds` prints one line per kind with its badges and runs nothing.
