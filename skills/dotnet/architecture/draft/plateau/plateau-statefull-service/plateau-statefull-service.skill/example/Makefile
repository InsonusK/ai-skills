SOLUTION := StatefullService.sln
CONFIGURATION := Release

.PHONY: restore build test unit-test mutation-test test-report test-and-report clean

WITH_CODE_COVERAGE ?= false
ONLY_DELTA ?= false
DELTA_BASE ?=

restore:
	dotnet restore $(SOLUTION)
	dotnet tool restore

build: restore
	dotnet build $(SOLUTION) --configuration $(CONFIGURATION) --no-restore

test: unit-test

unit-test: build
	WITH_CODE_COVERAGE=$(WITH_CODE_COVERAGE) scripts/unit-test.sh

mutation-test: restore
	ONLY_DELTA=$(ONLY_DELTA) DELTA_BASE=$(DELTA_BASE) scripts/mutation-test.sh

test-report:
	scripts/test-report.sh

test-and-report: WITH_CODE_COVERAGE := true
test-and-report: unit-test mutation-test test-report

clean:
	dotnet clean $(SOLUTION)
	find . -type d \( -name bin -o -name obj \) -exec rm -rf {} +
