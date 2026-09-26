# Makefile

Exposes the `unit-test`/`mutation-test`/`test-report`/`test-and-report` targets required by [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]].

```makefile
.PHONY: install build test unit-test mutation-test test-report test-and-report clean

WITH_CODE_COVERAGE ?= false
ONLY_DELTA ?= false
DELTA_BASE ?=

install:
	npm install

build: install
	npm run build

test: unit-test

unit-test: install
	WITH_CODE_COVERAGE=$(WITH_CODE_COVERAGE) scripts/unit-test.sh

mutation-test: install
	ONLY_DELTA=$(ONLY_DELTA) DELTA_BASE=$(DELTA_BASE) scripts/mutation-test.sh

test-report:
	scripts/test-report.sh

test-and-report: WITH_CODE_COVERAGE := true
test-and-report: unit-test mutation-test test-report

clean:
	npm run clean
	rm -rf tmp/result tmp/report public
```
