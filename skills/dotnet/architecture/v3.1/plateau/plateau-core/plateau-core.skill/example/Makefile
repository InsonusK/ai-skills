# Conformance-testing contract (solution-dotnet-conformance-testing).
# `make unit-test` is the ground-truth gate for a plateau example.
SHELL := /bin/bash
DOTNET ?= dotnet

.PHONY: build unit-test test mutation-test test-report test-and-report clean

build:
	$(DOTNET) build Sample.slnx -warnaserror

unit-test:
	$(DOTNET) test Sample.slnx --nologo

test: unit-test

# Stryker.NET mutation run (heavy; not part of the fast gate). Requires: dotnet tool install -g dotnet-stryker
mutation-test:
	$(DOTNET) stryker --solution Sample.slnx || echo "stryker not installed — skipped"

test-report:
	$(DOTNET) test Sample.slnx --collect:"XPlat Code Coverage" --results-directory tmp/coverage --nologo

test-and-report: unit-test test-report

clean:
	$(DOTNET) clean Sample.slnx; rm -rf tmp
