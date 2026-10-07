# Makefile

Exposes the `test-kind-unit`/`test-kind-mutation`/`test-report`/`test-and-report` targets required by [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]]. Runs against the whole solution, so every test project is covered by one invocation — no per-project target.

```makefile
SOLUTION := {Solution}.slnx
CONFIGURATION := Release

.PHONY: restore build run test clean

restore:
	dotnet restore $(SOLUTION)
	dotnet tool restore

build: restore
	dotnet build $(SOLUTION) --configuration $(CONFIGURATION) --no-restore

run: build
	dotnet run --project src/App/App.Host/App.Host.csproj --configuration $(CONFIGURATION) --no-build

# --- solution-conformance-testing-in-dotnet: the test kinds behind the shared contract ---
# Caller-facing targets and variables (test-kinds, test-kind-<kind>, test-report,
# test-readme-check, test-and-report; TEST_RUN_PURPOSE, DELTA_BASE, TEST_WORK_DIR,
# TEST_REPORT_DIR) come from tools/testing/testing.mk - see solution-conformance-testing.

TEST_KINDS           := unit mutation
TEST_BADGES_unit     := tests coverage
TEST_BADGES_mutation := mutation
include tools/testing/testing.mk

test: test-kind-unit

# unit: every test project in one `dotnet test`. Coverage is collected and reported
# only in a `report` run.
test-kind-unit: build
	@$(test-kind-begin)
	@if [ "$(TEST_RUN_PURPOSE)" = report ]; then $(call test-kind-mode,every test with coverage collected and reported); \
	else $(call test-kind-mode,every test - coverage not collected); fi
	@scripts/unit-test.sh

# mutation: Stryker.NET over the whole solution in a `report` run (the score never fails
# the run); in a `check` run only over code changed since DELTA_BASE, and skipped
# without one.
test-kind-mutation: restore
	@$(test-kind-begin)
	@if [ "$(TEST_RUN_PURPOSE)" = check ] && [ -z "$(DELTA_BASE)" ]; then \
		$(call test-kind-skip,a check run mutates only changed code and no DELTA_BASE was given); fi; \
	if [ "$(TEST_RUN_PURPOSE)" = check ]; then $(call test-kind-mode,mutating only code changed since $(DELTA_BASE)); \
	else $(call test-kind-mode,mutating the whole solution - the score never fails the run); fi; \
	scripts/mutation-test.sh

# test-report-build reads only the kinds' result/*.json (never a tool's native report
# format) and fills $(TEST_REPORT_DIR); `make test-report` calls it.
test-report-build:
	@scripts/test-report.sh

clean:
	dotnet clean $(SOLUTION)
	find . -type d \( -name bin -o -name obj \) -exec rm -rf {} +
	rm -rf tmp
```
