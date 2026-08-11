# Makefile

Exposes the `cucumber-test`/`mutation-test`/`result-page` targets required by [bdd-coverage-mutation-testing](skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md#make-command-contract).

```makefile
.PHONY: install test cucumber-test mutation-test result-page clean

WITH_CODE_COVERAGE ?= false
ONLY_DELTA ?= false
DELTA_BASE ?=

install:
	pip install -e ".[dev]"

test: cucumber-test

cucumber-test: install
	WITH_CODE_COVERAGE=$(WITH_CODE_COVERAGE) scripts/cucumber-test.sh

mutation-test: install
	ONLY_DELTA=$(ONLY_DELTA) DELTA_BASE=$(DELTA_BASE) scripts/mutation-test.sh

result-page:
	scripts/result-page.sh

clean:
	rm -rf tmp/result tmp/report public .coverage
```
