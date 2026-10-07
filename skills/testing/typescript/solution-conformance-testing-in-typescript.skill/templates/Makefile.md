# Makefile

Exposes the `test-kind-unit`/`test-kind-mutation`/`test-report`/`test-and-report` targets required by [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]].

```makefile
.PHONY: install build test clean

install:
	npm install

build: install
	npm run build

# --- solution-conformance-testing-in-typescript: the test kinds behind the shared contract ---
# Caller-facing targets and variables (test-kinds, test-kind-<kind>, test-report,
# test-readme-check, test-and-report; TEST_RUN_PURPOSE, DELTA_BASE, TEST_WORK_DIR,
# TEST_REPORT_DIR) come from tools/testing/testing.mk - see solution-conformance-testing.

TEST_KINDS           := unit mutation
TEST_BADGES_unit     := tests coverage
TEST_BADGES_mutation := mutation
include tools/testing/testing.mk

test: test-kind-unit

test-kind-unit: install
	@$(test-kind-begin)
	@if [ "$(TEST_RUN_PURPOSE)" = report ]; then $(call test-kind-mode,every test with coverage reported); \
	else $(call test-kind-mode,every test - coverage not reported); fi
	@scripts/unit-test.sh

test-kind-mutation: install
	@$(test-kind-begin)
	@if [ "$(TEST_RUN_PURPOSE)" = check ] && [ -z "$(DELTA_BASE)" ]; then \
		$(call test-kind-skip,a check run mutates only changed files and no DELTA_BASE was given); fi; \
	if [ "$(TEST_RUN_PURPOSE)" = check ]; then $(call test-kind-mode,mutating only files changed since $(DELTA_BASE)); \
	else $(call test-kind-mode,mutating the whole package - the score never fails the run); fi; \
	scripts/mutation-test.sh

test-report-build:
	@scripts/test-report.sh

clean:
	npm run clean
	rm -rf tmp
```
