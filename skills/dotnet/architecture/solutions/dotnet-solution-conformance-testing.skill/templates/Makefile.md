# Makefile

Exposes the `cucumber-test`/`mutation-test`/`result-page` targets required by [bdd-coverage-mutation-testing](skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md#make-command-contract).

```makefile
SOLUTION := {Module}.slnx
TEST_PROJECT := {Module}.Tests/{Module}.Tests.csproj
CONFIGURATION := Release

.PHONY: restore build test cucumber-test mutation-test result-page clean

WITH_CODE_COVERAGE ?= false
ONLY_DELTA ?= false
DELTA_BASE ?=

restore:
	dotnet restore $(SOLUTION)
	dotnet tool restore

build: restore
	dotnet build $(SOLUTION) --configuration $(CONFIGURATION) --no-restore

test: cucumber-test

cucumber-test: build
	WITH_CODE_COVERAGE=$(WITH_CODE_COVERAGE) scripts/cucumber-test.sh

mutation-test: restore
	ONLY_DELTA=$(ONLY_DELTA) DELTA_BASE=$(DELTA_BASE) scripts/mutation-test.sh

result-page:
	scripts/result-page.sh

clean:
	dotnet clean $(SOLUTION)
	find . -type d \( -name bin -o -name obj \) -exec rm -rf {} +
```
