SHELL := /bin/bash

# The kinds call pip, coverage and mutmut from PATH: `make init` creates the environment
# they find there first.
VENV := .venv
export PATH := $(CURDIR)/$(VENV)/bin:$(PATH)

.PHONY: init

# Run once after cloning.
init:
	python3 -m venv $(VENV)
	$(VENV)/bin/pip install --quiet -e ".[dev]"

# Testing contract: test-kinds, test-kind-<kind>, test-report, test-readme-check, test-and-report.
# The kinds are the scripts in tools/testing/kinds/ - see solution-conformance-testing.
include tools/testing/testing.mk
