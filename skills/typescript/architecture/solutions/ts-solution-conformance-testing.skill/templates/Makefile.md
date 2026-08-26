# Makefile

Exposes the `cucumber-test`/`mutation-test`/`result-page` targets required by [solution-conformance-testing](skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract).

```makefile
.PHONY: install build test cucumber-test mutation-test result-page clean

WITH_CODE_COVERAGE ?= false
ONLY_DELTA ?= false
DELTA_BASE ?=

install:
	npm install

build: install
	npm run build

test: cucumber-test

cucumber-test: install
	WITH_CODE_COVERAGE=$(WITH_CODE_COVERAGE) scripts/cucumber-test.sh

mutation-test: install
	ONLY_DELTA=$(ONLY_DELTA) DELTA_BASE=$(DELTA_BASE) scripts/mutation-test.sh

result-page:
	scripts/result-page.sh

clean:
	npm run clean
	rm -rf tmp/result tmp/report public
```
