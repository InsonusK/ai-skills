SHELL := /bin/bash
VENV := .venv
PYTHON := $(VENV)/bin/python

.PHONY: init run test lint

# Run once after cloning: a virtual environment with the package and its dev tools.
# The environment holds no symbolic link (--copies, and lib64 -> lib removed,
# which nothing reads), so the skill sync of the catalog can walk the example.
init:
	python3 -m venv --copies $(VENV)
	rm -f $(VENV)/lib64
	$(PYTHON) -m pip install --quiet -e '.[dev]'

# Runs the startup command once, planning its titles with the todo API's
# emulator (TODO_API_HOST=mock). Against a running todo service:
#   TODO_API_HOST=localhost TODO_API_HTTP_PORT=8080 make run
TODO_API_HOST ?= mock
run:
	TODO_API_HOST=$(TODO_API_HOST) STARTUP_TITLES="buy milk,walk the dog" $(PYTHON) -m todo

# Every Cucumber scenario of every package (pytest-bdd runs inside pytest).
# A real project adds its testing contract (kinds, reports, mutation) per
# solution-conformance-testing-in-python; this example leaves it out.
test:
	$(PYTHON) -m pytest -q -m "not status/todo and not status/broken"

lint:
	$(PYTHON) -m mypy
