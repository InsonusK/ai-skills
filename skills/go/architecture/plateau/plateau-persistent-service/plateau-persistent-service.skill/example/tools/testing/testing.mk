# tools/testing/testing.mk - the whole Makefile side of the testing contract
# (solution-conformance-testing). Copied verbatim into every project; never edited there.
# The project's Makefile needs one line:   include tools/testing/testing.mk
#
# A test kind is a script: tools/testing/kinds/<kind>.sh. Adding a kind is adding a script.
#
#   make test-kinds            list the kinds and the badges each declares; runs nothing
#   make test-kind-<kind>      run one kind
#   make test-report           build the report from what the kinds left
#   make test-readme-check     the README shows exactly the declared badges; runs no tests
#   make test-and-report       every kind, then the report

# What the run is for. check: decides whether a change may proceed (merge, publish) - must be fast.
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

export TEST_RUN_PURPOSE DELTA_BASE TEST_WORK_DIR TEST_REPORT_DIR TEST_README

.PHONY: test-kinds test-report test-readme-check test-and-report

test-kinds:
	@bash tools/testing/testing.sh kinds

test-kind-%:
	@bash tools/testing/testing.sh kind $*

test-report:
	@bash tools/testing/testing.sh report

test-readme-check:
	@bash tools/testing/testing.sh readme-check

test-and-report:
	@bash tools/testing/testing.sh all
